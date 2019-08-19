using Dapper.Contrib.Extensions;
using Paden.Aspects.Caching.Redis;
using Paden.Aspects.DAL.Entities;
using Paden.Aspects.Storage.MySQL;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static Paden.Aspects.Caching.Redis.CacheExtensions;

namespace Paden.Aspects.DAL
{
    public class StudentRepository
    {
        [Cache]
        [DbConnection]
        public Task<IEnumerable<Student>> GetAllAsync(IDbConnection connection = null)
        {
            return connection.GetAllAsync<Student>();
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
            return result;
        }
    }
}
