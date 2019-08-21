using Xunit;
using Paden.Aspects.DAL.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;
using Moq;
using System.Data;
using Paden.Aspects.Caching.Redis;
using static Paden.Aspects.Caching.Redis.CacheExtensions;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Paden.Aspects.DAL.Tests
{
    public class StudentRepositoryTests : IClassFixture<DatabaseFixture>, IDisposable
    {
        DatabaseFixture fixture;
        readonly ITestOutputHelper output;

        StudentRepository systemUnderTest;

        public StudentRepositoryTests(DatabaseFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.output = output;

            fixture.RecreateTables();

            systemUnderTest = new StudentRepository();
        }

        public void Dispose()
        {
            systemUnderTest.InvalidateCache(r => r.GetAllAsync(Any<IDbConnection>()));
        }

        [Fact]
        public async Task Should_Create_Update_Delete_Entity_And_Return_Proper_Results_Using_Database()
        {
            systemUnderTest.CacheEnabled = false;

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

        [Fact]
        public async Task GetAllAsync_Should_Not_Call_Database_On_Second_Call()
        {
            var student = new Student
            {
                Name = "Not Marko Padjen"
            };
            await systemUnderTest.InsertAsync(student);

            var swDatabase = new Stopwatch();
            swDatabase.Start();
            Assert.Equal(student.Name, (await systemUnderTest.GetAllAsync()).First().Name);
            swDatabase.Stop();

            output.WriteLine($"Database run time (ms): {swDatabase.ElapsedMilliseconds}");

            var connectionMock = Mock.Of<IDbConnection>();

            var swCache = new Stopwatch();
            swCache.Start();
            Assert.Equal(student.Name, (await systemUnderTest.GetAllAsync(connectionMock)).First().Name);
            swCache.Stop();

            Mock.Get(connectionMock).Verify(m => m.Open(), Times.Never);

            output.WriteLine($"Cache run time (ms): {swCache.ElapsedMilliseconds}");

            Assert.True(swCache.ElapsedMilliseconds < swDatabase.ElapsedMilliseconds);
        }

        [Fact]
        public async Task GetAllAsync_Should_Be_Called_Again_If_Entity_Updated()
        {
            var student = new Student
            {
                Id = 1,
                Name = "Not Marko Padjen"
            };
            var studentUpdated = new Student
            {
                Id = 1,
                Name = "Not Marko Padjen UPDATED"
            };
            await systemUnderTest.InsertAsync(student);

            Assert.Equal(student.Name, (await systemUnderTest.GetAllAsync()).First().Name);

            await systemUnderTest.UpdateAsync(studentUpdated);

            var connectionMock = fixture.GetConnectionFacade();

            Assert.Equal(studentUpdated.Name, (await systemUnderTest.GetAllAsync(connectionMock)).First().Name);

            Mock.Get(connectionMock).Verify(m => m.CreateCommand(), Times.Once);
        }
    }
}
