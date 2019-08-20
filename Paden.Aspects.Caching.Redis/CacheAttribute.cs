﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Paden.Aspects.Caching.Redis
{
    [PSerializable]
    [ProvideAspectRole(StandardRoles.Caching)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.TransactionHandling)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CacheAttribute : MethodInterceptionAspect
    {
        const int DefaultExpirySeconds = 5 * 60;

        static Lazy<IConfigurationRoot> config;
        static Lazy<IDatabase> db;

        public int ExpirySeconds = DefaultExpirySeconds;
        private TimeSpan? Expiry => ExpirySeconds == -1 ? (TimeSpan?)null : TimeSpan.FromSeconds(ExpirySeconds);

        public static bool IsEnabled { get; set; }

        static CacheAttribute()
        {
            config = new Lazy<IConfigurationRoot>(() => new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build());
            db = new Lazy<IDatabase>(() => ConnectionMultiplexer.Connect(config.Value["Redis:Server"]).GetDatabase());
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            if (!IsEnabled)
            {
                args.Proceed();
                return;
            }

            var key = GetKey(args.Method as MethodInfo, args.Arguments);
            var redisValue = db.Value.StringGet(key);

            if (redisValue.IsNullOrEmpty)
            {
                args.Proceed();
                db.Value.StringSet(key, JsonConvert.SerializeObject(args.ReturnValue), Expiry);
            }
            else
            {
                args.ReturnValue = JsonConvert.DeserializeObject(redisValue.ToString(), (args.Method as MethodInfo).ReturnType);
            }
        }

        public override async Task OnInvokeAsync(MethodInterceptionArgs args)
        {
            if (!IsEnabled)
            {
                await args.ProceedAsync();
                return;
            }

            var key = GetKey(args.Method as MethodInfo, args.Arguments);
            var redisValue = await db.Value.StringGetAsync(key);

            if (redisValue.IsNullOrEmpty)
            {
                await args.ProceedAsync();
                db.Value.StringSet(key, JsonConvert.SerializeObject(args.ReturnValue), Expiry);
            }
            else
            {
                args.ReturnValue = JsonConvert.DeserializeObject(redisValue.ToString(), (args.Method as MethodInfo).ReturnType.GenericTypeArguments[0]);
            }
        }

        private string GetKey(MethodInfo method, IList<object> values)
        {
            var parameters = method.GetParameters();
            var keyBuilder = GetKeyBuilder(method);
            keyBuilder.Append("(");
            foreach (var parameter in parameters)
            {
                AppendParameterValue(keyBuilder, parameter, values[parameter.Position]);
            }
            if (parameters.Any())
            {
                keyBuilder.Remove(keyBuilder.Length - 2, 2);
            }
            keyBuilder.Append(")");

            return keyBuilder.ToString();
        }

        public static void InvalidateCache<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            var methodCallExpression = expression.Body as MethodCallExpression;
            var keyBuilder = GetKeyBuilder(methodCallExpression.Method);
            var parameters = methodCallExpression.Method.GetParameters();

            var anyMethod = typeof(CacheExtensions).GetMethod(nameof(CacheExtensions.Any));

            keyBuilder.Append("(");
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var argument = methodCallExpression.Arguments[i];

                object value = null;

                if (argument is ConstantExpression)
                {
                    var constantArgument = argument as ConstantExpression;
                    value = constantArgument.Value;
                }
                else if (argument is MethodCallExpression)
                {
                    var methodArgument = argument as MethodCallExpression;
                    if (methodArgument.Method == anyMethod.MakeGenericMethod(methodArgument.Method.GetGenericArguments()))
                    {
                        value = "*";
                    }
                }

                AppendParameterValue(keyBuilder, parameter, value);
            }
            if (methodCallExpression.Arguments.Any())
            {
                keyBuilder.Remove(keyBuilder.Length - 2, 2);
            }
            keyBuilder.Append(")");
            
            db.Value.ScriptEvaluate(@"
                local keys = redis.call('keys', ARGV[1]) 
                for i=1, #keys, 5000 do 
                redis.call('del', unpack(keys, i, math.min(i + 4999, #keys)))
                end", values: new RedisValue[] { CacheExtensions.EscapeRedisString(keyBuilder.ToString()) });
        }

        private static StringBuilder GetKeyBuilder(MethodInfo method)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(method.ReturnType.FullName);
            keyBuilder.Append(" {");
            keyBuilder.Append(method.ReflectedType.AssemblyQualifiedName);
            keyBuilder.Append("}.");
            keyBuilder.Append(method.ReflectedType.FullName);
            keyBuilder.Append(".");
            keyBuilder.Append(method.Name);
            return keyBuilder;
        }

        private static void AppendParameterValue(StringBuilder keyBuilder, ParameterInfo parameter, object value)
        {
            keyBuilder.Append(parameter.ParameterType.FullName);
            keyBuilder.Append(" ");
            keyBuilder.Append(value == null ? "<NULL>" : value.ToString());
            keyBuilder.Append(", ");
        }
    }
}