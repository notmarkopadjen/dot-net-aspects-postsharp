using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Paden.Aspects.DAL;
using System;
using System.Data;
using System.Linq;

namespace Paden.Aspects.POC
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build();

            using (IDbConnection db = new MySqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                Console.WriteLine(new StudentRepository().ReadAll(db).First().Name);
            }
            Console.WriteLine(new string('-', 10));
            Console.WriteLine(new StudentRepository().ReadAll().First().Name);
            Console.WriteLine(new string('-', 10));
            Console.WriteLine(new StudentRepository().ReadAllAsync().Result.First().Name);
        }
    }
}
