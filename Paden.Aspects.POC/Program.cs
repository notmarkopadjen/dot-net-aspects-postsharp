using Paden.Aspects.DAL;
using Paden.Aspects.DAL.Entities;
using System;
using System.Linq;

namespace Paden.Aspects.POC
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new StudentRepository();
            repo.UpdateAsync(new Student
            {
                Id = 1,
                Name = $"Name {Guid.NewGuid()}"
            }).Wait();
            Console.WriteLine(repo.GetAllAsync().Result.First().Name);
        }
    }
}
