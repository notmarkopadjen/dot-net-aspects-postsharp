using Dapper;
using MySql.Data.MySqlClient;
using Paden.Aspects.DAL.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Paden.Aspects.DAL
{
    public class StudentRepository
    {
        public List<Student> ReadAll()
        {
            using (IDbConnection db = new MySqlConnection("Server=localhost;User ID=root;Password=password;Database=university"))
            {
                return db.Query<Student>($"select * from `{nameof(Student)}`").ToList();
            }
        }

    }
}
