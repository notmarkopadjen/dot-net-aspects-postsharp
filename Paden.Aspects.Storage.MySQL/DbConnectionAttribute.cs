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
    [PSerializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DbConnectionAttribute : MethodInterceptionAspect
    {
        const string DefaultConnectionStringName = "DefaultConnection";

        static Lazy<IConfigurationRoot> config;

        static DbConnectionAttribute()
        {
            config = new Lazy<IConfigurationRoot>(() => new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build());
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            if (args.Arguments.Last() != null)
            {
                args.Proceed();
                return;
            }

            using (IDbConnection db = new MySqlConnection(config.Value.GetConnectionString(DefaultConnectionStringName)))
            {
                args.Arguments.SetArgument(args.Arguments.Count - 1, db);
                args.Proceed();
            }
        }

        public override async Task OnInvokeAsync(MethodInterceptionArgs args)
        {
            if (args.Arguments.Last() != null)
            {
                await args.ProceedAsync();
                return;
            }

            using (IDbConnection db = new MySqlConnection(config.Value.GetConnectionString(DefaultConnectionStringName)))
            {
                args.Arguments.SetArgument(args.Arguments.Count - 1, db);
                await args.ProceedAsync();
            }
        }
    }
}
