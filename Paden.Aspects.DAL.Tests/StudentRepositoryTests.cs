using Xunit;
using Paden.Aspects.DAL.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;
using Paden.Aspects.Caching.Redis;

namespace Paden.Aspects.DAL.Tests
{
    public class StudentRepositoryTests : IClassFixture<DatabaseFixture>, IDisposable
    {
        StudentRepository systemUnderTest;

        public StudentRepositoryTests()
        {
            systemUnderTest = new StudentRepository();
        }

        public void Dispose()
        {
            CacheAttribute.IsEnabled = true;
        }

        [Fact]
        public async Task Shuold_Create_Entity_Update_Delete_And_Return_Proper_Results_Using_Database()
        {
            CacheAttribute.IsEnabled = false;

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

            await systemUnderTest.DeleteAsync(new Student
            {
                Id = 1
            });

            Assert.False((await systemUnderTest.GetAllAsync()).Any());
        }
    }
}
