using Moq;
using Xunit;
using AutoFixture;
using FluentAssertions;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using web_api_crud.Controllers;
using web_api_crud.Models;
using Microsoft.EntityFrameworkCore;

namespace api_test
{
    public class StudentApiControllerTest
    {
        private readonly Mock<ApiStudentDatabaseContext> mockContext;
        private readonly StudentApiController controller;
        private readonly Fixture fixture;

        public StudentApiControllerTest()
        {
            mockContext = new Mock<ApiStudentDatabaseContext>();
            controller = new StudentApiController(mockContext.Object);
            fixture = new Fixture();
        }

        // Update the GetStudents_ReturnsOkResult test
        [Fact]
        public async Task GetStudents_ReturnsOkResult()
        {
            // Arrange
            var students = fixture.CreateMany<Student>(5).ToList();

            // Set up the mock context to return students when ToListAsync is called
            var mockDbSet = new Mock<DbSet<Student>>();
            mockDbSet.As<IQueryable<Student>>().Setup(m => m.Provider).Returns(students.AsQueryable().Provider);
            mockDbSet.As<IQueryable<Student>>().Setup(m => m.Expression).Returns(students.AsQueryable().Expression);
            mockDbSet.As<IQueryable<Student>>().Setup(m => m.ElementType).Returns(students.AsQueryable().ElementType);
            mockDbSet.As<IQueryable<Student>>().Setup(m => m.GetEnumerator()).Returns(students.AsQueryable().GetEnumerator());

            mockContext.Setup(c => c.Students).Returns(mockDbSet.Object);

            // Act
            var result = await controller.GetStudents();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeOfType<List<Student>>();
            okResult.Value.Should().BeEquivalentTo(students);
        }


        [Fact]
        public async Task GetStudentsById_WithValidId_ReturnsStudent()
        {
            // Arrange
            var studentId = "1";
            var student = fixture.Create<Student>();
            mockContext.Setup(c => c.Students.FindAsync(studentId)).ReturnsAsync(student);

            // Act
            var result = await controller.GetStudentsById(studentId);

            // Assert
            result.Should().BeOfType<ActionResult<Student>>();
            result.Value.Should().BeEquivalentTo(student);
        }

        [Fact]
        public async Task GetStudentsById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var studentId = "100"; // An ID that doesn't exist
            mockContext.Setup(c => c.Students.FindAsync(studentId)).ReturnsAsync((Student)null);

            // Act
            var result = await controller.GetStudentsById(studentId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateStudent_ValidStudent_ReturnsOkResult()
        {
            // Arrange
            var newStudent = fixture.Create<Student>();

            // Act
            var result = await controller.CreateStudent(newStudent);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(newStudent);
        }
        [Fact]
        public async Task UpdateStudent_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var studentId = "1";
            var studentToUpdate = fixture.Create<Student>();

            mockContext.Setup(c => c.Students.FindAsync(studentId)).ReturnsAsync(studentToUpdate);

            // Act
            var result = await controller.UpdateStudent(studentId, studentToUpdate);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(studentToUpdate);
        }

        [Fact]
        public async Task UpdateStudent_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var studentId = "100"; // An ID that doesn't exist
            var studentToUpdate = fixture.Create<Student>();

            // Act
            var result = await controller.UpdateStudent(studentId, studentToUpdate);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async Task DeleteStudent_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var studentId = "1";
            var studentToDelete = fixture.Create<Student>();

            mockContext.Setup(c => c.Students.FindAsync(studentId)).ReturnsAsync(studentToDelete);

            // Act
            var result = await controller.DeleteStudent(studentId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(studentToDelete);
        }

        [Fact]
        public async Task DeleteStudent_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var studentId = "100"; // An ID that doesn't exist

            mockContext.Setup(c => c.Students.FindAsync(studentId)).ReturnsAsync((Student)null);

            // Act
            var result = await controller.DeleteStudent(studentId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }



       
    }
}
public static class MockExtensions
{
    public static DbSet<T> ReturnsDbSet<T>(this Mock<ApiStudentDatabaseContext> mockContext, IEnumerable<T> data) where T : class
    {
        var mockDbSet = new Mock<DbSet<T>>();
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        mockContext.Setup(c => c.Set<T>()).Returns(mockDbSet.Object);
        return mockDbSet.Object;
    }
}
