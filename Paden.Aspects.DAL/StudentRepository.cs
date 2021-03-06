﻿using Dapper.Contrib.Extensions;
using Paden.Aspects.Caching.Redis;
using Paden.Aspects.DAL.Entities;
using Paden.Aspects.Storage.MySQL;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static Paden.Aspects.Caching.Redis.CacheExtensions;

namespace Paden.Aspects.DAL
{
    public class StudentRepository : ICacheAware
    {
        /// <summary>
        /// Use cache switch. Not required, convenient for testing
        /// </summary>
        public bool CacheEnabled { get; set; } = true;

        [DbConnection]
        public void ReCreateTable(IDbConnection connection = null)
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = Student.ReCreateStatement;
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        [Cache]
        [DbConnection]
        public Task<IEnumerable<Student>> GetAllAsync(IDbConnection connection = null)
        {
            return connection.GetAllAsync<Student>();
        }

        [Cache]
        [DbConnection]
        public Task<Student> GetAsync(int id, IDbConnection connection = null)
        {
            return connection.GetAsync<Student>(id);
        }

        [DbConnection]
        public async Task<int> InsertAsync(Student student, IDbConnection connection = null)
        {
            var result = await connection.InsertAsync(student);
            this.InvalidateCache(r => r.GetAllAsync(Any<IDbConnection>()));
            return result;
        }

        [DbConnection]
        public async Task<bool> UpdateAsync(Student student, IDbConnection connection = null)
        {
            var result = await connection.UpdateAsync(student);
            this.InvalidateCache(r => r.GetAllAsync(Any<IDbConnection>()));
            this.InvalidateCache(r => r.GetAsync(student.Id, Any<IDbConnection>()));
            return result;
        }

        [DbConnection]
        public async Task<bool> DeleteAsync(Student student, IDbConnection connection = null)
        {
            var result = await connection.DeleteAsync(student);
            this.InvalidateCache(r => r.GetAllAsync(Any<IDbConnection>()));
            this.InvalidateCache(r => r.GetAsync(student.Id, Any<IDbConnection>()));
            return result;
        }
    }
}
