using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Paden.Aspects.Storage.MySQL
{
    [PSerializable]
    [ProvideAspectRole(StandardRoles.TransactionHandling)]
    [AspectRoleDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DbConnectionAttribute : MethodInterceptionAspect
    {
        const string DefaultConnectionStringName = "DefaultConnection";

        static Lazy<IConfigurationRoot> config;

        static string connectionString;
        public static string ConnectionString
        {
            get { return connectionString ?? config.Value.GetConnectionString(DefaultConnectionStringName); }
            set { connectionString = value; }
        }

        static DbConnectionAttribute()
        {
            config = new Lazy<IConfigurationRoot>(() => new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build());
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            var i = GetArgumentIndex(args);
            if (!i.HasValue)
            {
                args.Proceed();
                return;
            }

            using (IDbConnection db = new MySqlConnection(ConnectionString))
            {
                args.Arguments.SetArgument(i.Value, db);
                args.Proceed();
            }
        }

        public override async Task OnInvokeAsync(MethodInterceptionArgs args)
        {
            var i = GetArgumentIndex(args);
            if (!i.HasValue)
            {
                await args.ProceedAsync();
                return;
            }

            using (IDbConnection db = new MySqlConnection(ConnectionString))
            {
                args.Arguments.SetArgument(i.Value, db);
                await args.ProceedAsync();
            }
        }

        private int? GetArgumentIndex(MethodInterceptionArgs args)
        {
            var parameters = args.Method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.ParameterType == typeof(IDbConnection)
                    && parameter.IsOptional
                    && args.Arguments[i] == null)
                {
                    return i;
                }
            }
            return null;
        }
    }
}
