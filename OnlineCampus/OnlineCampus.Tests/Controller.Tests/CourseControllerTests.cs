using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineCampus.Controllers;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using OnlineCampus.ViewModels;

namespace OnlineCampus.Tests.Controller.Tests
{
    public class CourseControllerTests
    {
        private readonly Mock<ICourseRepository> mockCourseRepository;
        private readonly Mock<ILogger<CourseController>> mockLogger;
        private CourseController controller;

        public CourseControllerTests() 
        { 
            mockCourseRepository = new Mock<ICourseRepository>();
            mockLogger = new Mock<ILogger<CourseController>>();
            controller = new CourseController(mockLogger.Object, mockCourseRepository.Object);
        }

        [Fact]
        public void Create_ReturnsViewResult()
        {
            // Arrange
            var mockRepo = new Mock<ICourseRepository>();
            var controller = new CourseController(mockLogger.Object, mockRepo.Object);

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public void Create_ModelStateInvalid_ReturnsViewWithViewModel()
        {
            // Arrange
            var controller = new CourseController(mockLogger.Object, mockCourseRepository.Object);
            controller.ModelState.AddModelError("Error", "Model state is invalid");
            var viewModel = new CourseViewModel();

            // Act
            var result = controller.Create(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(viewModel, result.Model);
        }

        [Fact]
        public void Create_SuccessfulCreation_RedirectsToToken()
        {
            // Arrange
            var controller = new CourseController(mockLogger.Object, mockCourseRepository.Object);
            var viewModel = new CourseViewModel
            {
                Code = "123",
                Name = "Physics",
                Description = "Natural Science",
                Credits = 123
            };

            // Act
            var result = controller.Create(viewModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            mockCourseRepository.Verify(repo => repo.InsertCourse(It.Is<Course>(c => c.Code == "123" && c.Name == "Physics" && c.Description == "Natural Science" && c.Credits == 123)), Times.Once);
            mockCourseRepository.Verify(repo => repo.Save(), Times.Once);
        }

        [Fact]
        public void Create_DbUpdateException_SetsViewBagMessage()
        {
            // Arrange
            var controller = new CourseController(mockLogger.Object, mockCourseRepository.Object);
            var viewModel = new CourseViewModel
            {
                Code = "123",
                Name = "Physics",
                Description = "Natural Science",
                Credits = 123
            };
            mockCourseRepository.Setup(repo => repo.InsertCourse(It.IsAny<Course>()))
                .Throws(new DbUpdateException("Test exception", new Exception("Inner exception")));

            // Act
            var result = controller.Create(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("An error occurred while inserting data into the database.", controller.ViewBag.Message);
            Assert.Equal("An error occurred while inserting data into the database.", controller.ModelState[""].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Edit_CourseNotFound_SetsViewBagMessageAndReturnsView()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var mockRepository = new Mock<ICourseRepository>();
            mockRepository.Setup(repo => repo.GetCourseByIdAsync(courseId))
                .ReturnsAsync((Course) null);
            var controller = new CourseController(mockLogger.Object, mockRepository.Object);

            // Act
            var result = await controller.Edit(courseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.Equal("The Course has not been found.", controller.ViewBag.Message);
        }

        [Fact]
        public async Task Edit_CourseFound_ReturnsViewWithModel()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var mockRepository = new Mock<ICourseRepository>();
            var course = new Course
            {
                CourseId = courseId,
                Code = "123",
                Name = "Physics",
                Description = "Natural Science",
                Credits = 123,
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };
            mockRepository.Setup(repo => repo.GetCourseByIdAsync(courseId))
                .ReturnsAsync(course);
            var controller = new CourseController(mockLogger.Object, mockRepository.Object);

            // Act
            var result = await controller.Edit(courseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CourseDetailViewModel>(viewResult.Model);
            Assert.Equal(course.CourseId, model.CourseId);
            Assert.Equal(course.Name, model.Name);
            Assert.Equal(course.Code, model.Code);
            Assert.Equal(course.Description, model.Description);
            Assert.Equal(course.Credits, model.Credits);
            Assert.Equal(course.RowVersion, model.RowVersion);
        }

        [Fact]
        public async Task Edit_DbUpdateException_SetsModelErrorAndReturnsView() 
        { 
            // Arrange
            var courseId = Guid.NewGuid();
            var mockRepository = new Mock<ICourseRepository>();
            mockRepository.Setup(repo => repo.GetCourseByIdAsync(courseId))
                .ThrowsAsync(new DbUpdateException("Test exception", new Exception("Inner exception")));
            var controller = new CourseController(mockLogger.Object, mockRepository.Object);

            // Act
            var result = await controller.Edit(courseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            Assert.Equal("An error occurred while retrieving data from the database.", controller.ViewBag.Message);
            Assert.True(controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task Edit_ModelStateInvalid_ReturnsViewWithViewModel()
        {
            // Arrange
            var controller = new CourseController(mockLogger.Object, mockCourseRepository.Object);
            controller.ModelState.AddModelError("Error", "Model state is invalid.");
            var viewModel = new CourseDetailViewModel();

            // Act
            var result = await controller.Edit(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(viewModel, result.Model);
        }

        [Fact]
        public async Task Edit_CourseNotFound_SetsViewBagMessage()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var controller = new CourseController(mockLogger.Object, mockCourseRepository.Object);
            var viewModel = new CourseDetailViewModel { CourseId = courseId };
            mockCourseRepository.Setup(repo => repo.GetCourseByIdAsync(courseId))
                .ReturnsAsync((Course)null);

            // Act
            var result = await controller.Edit(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Course was not found.", controller.ViewBag.Message);
        }

        [Fact]
        public async Task Edit_CourseDataUnchanged_AddModelErrorAnDReturnsView()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var controller = new CourseController(mockLogger.Object, mockCourseRepository.Object);
            var viewModel = new CourseDetailViewModel
            {
                CourseId = courseId,
                Code = "Test",
                Name = "Test",
                Description = "Test",
                Credits = 123
            };
            var course = new Course
            {
                CourseId = courseId,
                Code = "Test",
                Name = "Test",
                Description = "Test",
                Credits = 123
            };
            mockCourseRepository.Setup(repo => repo.GetCourseByIdAsync(courseId)).ReturnsAsync(course);

            // Act
            var result = await controller.Edit(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(viewModel, result.Model);
            Assert.Equal("Data has not been modified.", controller.ModelState[string.Empty].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Edit_DbUpdateConcurrencyException_HandlesConcurrency()
        {
            // AArrange
            var courseId = Guid.NewGuid();
            var controller = new CourseController(mockLogger.Object, mockCourseRepository.Object);
            var viewModel = new CourseDetailViewModel
            {
                CourseId = courseId,
                Code = "Test",
                Name = "Test",
                Description = "Test",
                Credits = 123
            };
            var course = new Course
            {
                CourseId = courseId,
                Code = "Test",
                Name = "Test",
                Description = "Test",
                Credits = 123,
                RowVersion = viewModel.RowVersion
            };
            mockCourseRepository.Setup(repo => repo.GetCourseByIdAsync(courseId)).ReturnsAsync((Course)course);
            mockCourseRepository.Setup(repo => repo.UpdateCourse(course)).Throws(new DbUpdateConcurrencyException());

            // Act
            var result = await controller.Edit(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(viewModel, result.Model);
            Assert.Contains(controller.ModelState[string.Empty].Errors, e => e.ErrorMessage.Contains("Data has not been modified"));
        }

        [Fact]
        public async Task Edit_DbUpdateException_SetsViewBagMessage()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var controller = new CourseController(mockLogger.Object, mockCourseRepository.Object);
            var viewModel = new CourseDetailViewModel
            {
                CourseId = courseId
            };
            mockCourseRepository.Setup(repo => repo.GetCourseByIdAsync(courseId))
                .ThrowsAsync(new DbUpdateException());

            // Act
            var result = await controller.Edit(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("An error occurred while editing data in the database.", controller.ViewBag.Message);
        }

        [Fact]
        public async Task Delete_StudentFound_ReturnsViewWithModel()
        {
            var courseId = Guid.NewGuid();
            var mockRepository = new Mock<ICourseRepository>();
            var course = new Course
            {
                CourseId = courseId,
                Code = "Test",
                Name = "Test",
                Description = "Test",
                Credits = 123,
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };
            mockRepository.Setup(repo => repo.GetCourseByIdAsync(courseId))
                .ReturnsAsync(course);
            var controller = new CourseController(mockLogger.Object, mockRepository.Object);

            // Act
            var result = await controller.Delete(courseId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CourseDetailViewModel>(viewResult.Model);
            Assert.Equal(course.CourseId, model.CourseId);
            Assert.Equal(course.Name, model.Name);
            Assert.Equal(course.Code, model.Code);
            Assert.Equal(course.Description, model.Description);
            Assert.Equal(course.Credits, model.Credits);
            Assert.Equal(course.RowVersion, model.RowVersion);
        }

    }
}
