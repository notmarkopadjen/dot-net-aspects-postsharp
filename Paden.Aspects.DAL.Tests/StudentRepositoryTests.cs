using Moq;
using Paden.Aspects.Caching.Redis;
using Paden.Aspects.DAL.Entities;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Paden.Aspects.Caching.Redis.CacheExtensions;

namespace Paden.Aspects.DAL.Tests
{
    public class StudentRepositoryTests : IClassFixture<DatabaseFixture>, IDisposable
    {
        DatabaseFixture fixture;
        readonly ITestOutputHelper output;

        StudentRepository systemUnderTest;

        const int studentId = 1;

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
            systemUnderTest.InvalidateCache(r => r.GetAsync(studentId, Any<IDbConnection>()));
        }

        [Fact]
        public async Task Repository_Should_Create_Update_Delete_Entity_And_Return_Proper_Results_Using_Database()
        {
            // Disable cache to test database calls only
            systemUnderTest.CacheEnabled = false;

            await systemUnderTest.InsertAsync(new Student
            {
                Name = "Not Marko Padjen"
            });

            string newName;

            await systemUnderTest.UpdateAsync(new Student
            {
                Id = studentId,
                Name = newName = $"Name {Guid.NewGuid()}"
            });

            // Checking if database calls return updated name
            Assert.Equal(newName, (await systemUnderTest.GetAllAsync()).First().Name);

            await systemUnderTest.DeleteAsync(new Student
            {
                Id = studentId
            });

            // Checking if database table is empty after entity deletion
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
            // Gets entity from database and records run time
            Assert.Equal(student.Name, (await systemUnderTest.GetAllAsync()).First().Name);
            swDatabase.Stop();

            output.WriteLine($"Database run time (ms): {swDatabase.ElapsedMilliseconds}");

            var connectionMock = Mock.Of<IDbConnection>();

            var swCache = new Stopwatch();
            swCache.Start();
            // Gets entity from cache and records run time
            Assert.Equal(student.Name, (await systemUnderTest.GetAllAsync(connectionMock)).First().Name);
            swCache.Stop();

            // Ensure that database was not used
            Mock.Get(connectionMock).Verify(m => m.CreateCommand(), Times.Never);

            output.WriteLine($"Cache run time (ms): {swCache.ElapsedMilliseconds}");

            // Check if database call was slower that cache call
            Assert.True(swCache.Elapsed < swDatabase.Elapsed);
        }

        [Fact]
        public async Task GetAllAsync_Should_Be_Called_Again_If_Entity_Updated()
        {
            var student = new Student
            {
                Id = studentId,
                Name = "Not Marko Padjen"
            };
            var studentUpdated = new Student
            {
                Id = studentId,
                Name = "Not Marko Padjen UPDATED"
            };
            await systemUnderTest.InsertAsync(student);

            // Gets entities from database, saves to cache
            Assert.Equal(student.Name, (await systemUnderTest.GetAllAsync()).First().Name);

            // Updates entity, cache should be cleared
            await systemUnderTest.UpdateAsync(studentUpdated);

            var connectionMock = fixture.GetConnectionFacade();

            // Gets from database again
            Assert.Equal(studentUpdated.Name, (await systemUnderTest.GetAllAsync(connectionMock)).First().Name);

            // Ensures that database was not called
            Mock.Get(connectionMock).Verify(m => m.CreateCommand(), Times.Once);
        }


        [Fact]
        public async Task Get_Should_Call_Database_If_Entity_Not_Dirty_Otherwise_Read_From_Cache()
        {
            var student = new Student
            {
                Id = studentId,
                Name = "Not Marko Padjen"
            };
            var studentUpdated = new Student
            {
                Id = studentId,
                Name = "Not Marko Padjen UPDATED"
            };
            await systemUnderTest.InsertAsync(student);

            // Gets entity by id, should save in cache
            Assert.Equal(student.Name, (await systemUnderTest.GetAsync(studentId)).Name);

            // Updates entity by id, should invalidate cache
            await systemUnderTest.UpdateAsync(studentUpdated);

            var connectionMock = fixture.GetConnectionFacade();

            // Gets entity by id, ensures that it is the expected one
            Assert.Equal(studentUpdated.Name, (await systemUnderTest.GetAsync(studentId, connectionMock)).Name);

            // Ensures that database was used for the call
            Mock.Get(connectionMock).Verify(m => m.CreateCommand(), Times.Once);

            var connectionMockUnused = fixture.GetConnectionFacade();

            // Calls again, should read from cache
            Assert.Equal(studentUpdated.Name, (await systemUnderTest.GetAsync(studentId, connectionMockUnused)).Name);

            // Ensures that database was not used
            Mock.Get(connectionMockUnused).Verify(m => m.CreateCommand(), Times.Never);
        }
    }
}
