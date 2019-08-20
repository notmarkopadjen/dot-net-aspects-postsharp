using Xunit;
using Paden.Aspects.DAL.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Paden.Aspects.DAL.Tests
{
    public class StudentRepositoryTests : IClassFixture<DatabaseFixture>
    {
        StudentRepository systemUnderTest;

        public StudentRepositoryTests(DatabaseFixture fixture)
        {
            systemUnderTest = new StudentRepository();
        }

        [Fact]
        public async Task Shuold_Create_Entity_Update_And_Return_Proper_Results()
        {
            await systemUnderTest.InsertAsync(new Student
            {
                Name = "Not Marko Padjen"
            });

            string newName;

            await systemUnderTest.UpdateAsync(new Student
            {
                Id = 1,
                Name = newName = $"Name {Guid.NewGuid()}"
            });

            Assert.Equal(newName, (await systemUnderTest.GetAllAsync()).First().Name);
        }
    }
}
