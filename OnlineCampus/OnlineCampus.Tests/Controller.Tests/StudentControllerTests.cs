using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineCampus.Controllers;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using OnlineCampus.ViewModels;

namespace OnlineCampus.Tests.Controller.Tests
{
    public class StudentControllerTests
    {
        [Fact]
        public void Create_ReturnsViewResult()
        {
            // Arrange
            var mockRepo = new Mock<IStudentRepository>();
            var controller = new StudentController(mockRepo.Object);

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);  // Ensures the result is a ViewResult
            Assert.Null(viewResult.ViewName);    // Ensures the view name is null, meaning it uses the default view.
        }

        [Fact]
        public async Task Edit_StudentNotFound_SetsViewBagMessageAndReturnsView()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var mockRepository = new Mock<IStudentRepository>();
            mockRepository.Setup(repo => repo.GetStudentByIdAsync(studentId))
                .ReturnsAsync((Student)null);
            var controller = new StudentController(mockRepository.Object);

            // Act
            var result = await controller.Edit(studentId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.Equal("The Student has not been found.", controller.ViewBag.Message);
        }

        [Fact]
        public async Task Edit_StudentFound_ReturnsViewWithModel()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var mockRepository = new Mock<IStudentRepository>();
            var student = new Student
            {
                StudentId = studentId,
                FirstName = "John",
                LastName = "Doe",
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };
            mockRepository.Setup(repo => repo.GetStudentByIdAsync(studentId))
                .ReturnsAsync(student);
            var controller = new StudentController(mockRepository.Object);

            // Act
            var result = await controller.Edit(studentId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StudentDetailViewModel>(viewResult.Model);
            Assert.Equal(student.StudentId, model.StudentId);
            Assert.Equal(student.FirstName, model.FirstName);
            Assert.Equal(student.LastName, model.LastName);
            Assert.Equal(student.RowVersion, model.RowVersion);
        }

        [Fact]
        public async Task Edit_DbUpdateException_SetsModelErrorAndReturnsView()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var mockRepository = new Mock<IStudentRepository>();
            mockRepository.Setup(repo => repo.GetStudentByIdAsync(studentId))
                .ThrowsAsync(new DbUpdateException("Test exception", new Exception("Inner exception")));
            var controller = new StudentController(mockRepository.Object);

            // Act
            var result = await controller.Edit(studentId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model); // ensure no model is returned
            Assert.Equal("An error occurred while retrieving data from the database.", controller.ViewBag.Message); // Ensure ViewBag.Message is set
            Assert.True(controller.ModelState.ContainsKey("")); // Ensure a model error is added
        }

        [Fact]
        public async Task Delete_StudentFound_ReturnsViewWithModel()
        {
            var studentId = Guid.NewGuid();
            var mockRepository = new Mock<IStudentRepository>();
            var student = new Student
            {
                StudentId = studentId,
                FirstName = "John",
                LastName = "Doe",
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };
            mockRepository.Setup(repo => repo.GetStudentByIdAsync(studentId))
                .ReturnsAsync(student);
            var controller = new StudentController(mockRepository.Object);

            // Act
            var result = await controller.Delete(studentId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StudentDetailViewModel>(viewResult.Model);
            Assert.Equal(student.StudentId, model.StudentId);
            Assert.Equal(student.FirstName, model.FirstName);
            Assert.Equal(student.LastName, model.LastName);
            Assert.Equal(student.RowVersion, model.RowVersion);
        }
    }
}