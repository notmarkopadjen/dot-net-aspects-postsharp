using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Paden.Aspects.DAL.Entities;
using Paden.Aspects.Storage.MySQL;
using System;

namespace Paden.Aspects.DAL.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public MySqlConnection Connection { get; private set; }
        public readonly string DatabaseName = $"integration_test_{Guid.NewGuid():N}";

        public DatabaseFixture()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false).Build();
            var connectionString = config.GetConnectionString("DefaultConnection");
            Connection = new MySqlConnection(connectionString);

            Connection.Open();
            new MySqlCommand($"CREATE DATABASE `{DatabaseName}`;", Connection).ExecuteNonQuery();
            Connection.ChangeDatabase(DatabaseName);

            new MySqlCommand(Student.ReCreateStatement, Connection).ExecuteNonQuery();

            DbConnectionAttribute.ConnectionString = $"{connectionString};Database={DatabaseName}";
        }

        public void Dispose()
        {
            try
            {
                new MySqlCommand($"DROP DATABASE IF EXISTS `{DatabaseName}`;", Connection).ExecuteNonQuery();
            }
            catch (Exception)
            {
                // ignored
            }
            Connection.Close();
        }
    }
}
