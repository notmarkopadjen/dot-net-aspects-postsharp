using Microsoft.Extensions.Configuration;
using Moq;
using MySql.Data.MySqlClient;
using Paden.Aspects.DAL.Entities;
using Paden.Aspects.Storage.MySQL;
using System;
using System.Data;

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

            DbConnectionAttribute.ConnectionString = $"{connectionString};Database={DatabaseName}";
        }

        public void RecreateTables()
        {
            new MySqlCommand(Student.ReCreateStatement, Connection).ExecuteNonQuery();
        }

        public IDbConnection GetConnectionFacade()
        {
            var connectionMock = Mock.Of<IDbConnection>();
            Mock.Get(connectionMock).Setup(m => m.CreateCommand()).Returns(Connection.CreateCommand()).Verifiable();
            Mock.Get(connectionMock).SetupGet(m => m.State).Returns(ConnectionState.Open).Verifiable();
            return connectionMock;
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
