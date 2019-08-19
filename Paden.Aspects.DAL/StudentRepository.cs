using Dapper;
using Paden.Aspects.DAL.Entities;
using Paden.Aspects.Storage.MySQL;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Paden.Aspects.DAL
{
    public class StudentRepository
    {
        [DbConnection]
        public IEnumerable<Student> ReadAll(IDbConnection connection = null)
        {
            return connection.Query<Student>($"select * from `{nameof(Student)}`");
        }

        [DbConnection]
        public Task<IEnumerable<Student>> ReadAllAsync(IDbConnection connection = null)
        {
            return connection.QueryAsync<Student>($"select * from `{nameof(Student)}`");
        }
    }
}
