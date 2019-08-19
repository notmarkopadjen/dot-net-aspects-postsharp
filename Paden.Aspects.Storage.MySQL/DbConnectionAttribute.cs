using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PostSharp.Aspects;
using PostSharp.Serialization;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Paden.Aspects.Storage.MySQL
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    [PSerializable]
    public class DbConnectionAttribute : MethodInterceptionAspect
    {
        static Lazy<IConfigurationRoot> config;

        static DbConnectionAttribute()
        {
            config = new Lazy<IConfigurationRoot>(() => new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build());
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            var connectionArgument = args.Arguments.Last();
            if (connectionArgument != null)
            {
                base.OnInvoke(args);
                return;
            }

            using (IDbConnection db = new MySqlConnection(config.Value.GetConnectionString("DefaultConnection")))
            {
                args.Arguments.SetArgument(args.Arguments.Count - 1, db);   
                base.OnInvoke(args);
                return;
            }
        }

        public override Task OnInvokeAsync(MethodInterceptionArgs args)
        {
            return base.OnInvokeAsync(args);
        }
    }
}
