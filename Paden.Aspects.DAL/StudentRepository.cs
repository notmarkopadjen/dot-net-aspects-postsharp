using Dapper;
using Paden.Aspects.DAL.Entities;
using Paden.Aspects.Storage.MySQL;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Paden.Aspects.DAL
{
    public class StudentRepository
    {
        [DbConnection]
        public List<Student> ReadAll(IDbConnection connection = null)
        {
            return connection.Query<Student>($"select * from `{nameof(Student)}`").ToList();
        }
    }
}
