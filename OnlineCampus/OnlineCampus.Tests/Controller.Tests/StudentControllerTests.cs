using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
        private Mock<IStudentRepository> mockStudentRepository;
        private StudentController controller;

        public StudentControllerTests()
        {
            mockStudentRepository = new Mock<IStudentRepository>();
            controller = new StudentController(mockStudentRepository.Object);
        }

        [Fact]
        public async Task Index_SortOrderNameDesc_SortsStudentsByNameDescending()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student {FirstName = "John", LastName = "Smith"},
                new Student {FirstName = "Jane", LastName = "Doe"}
            }.AsQueryable();

            mockStudentRepository.Setup(repo => repo.GetStudents()).Returns(students);

            // Act
            var result = await controller.Index("name_desc", null, null, 1) as ViewResult;
            var model = result.Model as PaginatedList<Student>;

            // Assert
            Assert.NotNull(model);
            Assert.Equal("Smith", model[0].LastName);
            Assert.Equal("Doe", model[1].LastName);
        }

        [Fact]
        public async Task Index_SearchStringFilterStudents()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student {FirstName = "John", LastName = "Smith"},
                new Student {FirstName = "Jane", LastName = "Doe"}
            }.AsQueryable();

            mockStudentRepository.Setup(repo => repo.GetStudents()).Returns(students);

            // Act
            var result = await controller.Index(null, "Doe", null, 1) as ViewResult;
            var model = result.Model as PaginatedList<Student>;

            // Assert
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("Doe", model[0].LastName);
        }

        [Fact]
        public async Task Index_PaginationWorksCorrectly()
        {
            // Arrange
            var students = new List<Student>();
            for (int i = 1; i <= 20; i++)
            {
                students.Add(new Student { FirstName = "FirstName" + i, LastName = "LastName" + i });
            }
            mockStudentRepository.Setup(repo => repo.GetStudents()).Returns(students.AsQueryable());

            // Act
            var result = await controller.Index(null, null, null, 2) as ViewResult;
            var model = result.Model as PaginatedList<Student>;

            // Assert
            Assert.NotNull(model);
            Assert.Equal(8, model.Count);
            Assert.Equal("FirstName9", model[0].FirstName);
        }

        [Fact]
        public async Task Index_DbUpdateException_SetsViewBagMessage()
        {
            // Arrange
            mockStudentRepository.Setup(repo => repo.GetStudents())
                .Throws(new DbUpdateException("Test exception", new Exception("Inner exception")));

            // Act
            var result = await controller.Index(null, null, null, 1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("An error occurred while retrieving data from the database.", controller.ViewBag.Message);
            Assert.Equal("An error occurred while retrieving data from the database.", controller.ModelState[""].Errors[0].ErrorMessage);

        }

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
        public void Create_ModelStateInvalid_ReturnsViewWithViewModel()
        {
            // Arrange
            var controller = new StudentController(mockStudentRepository.Object);
            controller.ModelState.AddModelError("Error", "Model state is invalid");
            var viewModel = new StudentViewModel();

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
            var controller = new StudentController(mockStudentRepository.Object);
            var viewModel = new StudentViewModel { FirstName = "John", LastName = "Doe" };

            // Act
            var result = controller.Create(viewModel) as RedirectToActionResult;

            // AAssert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            mockStudentRepository.Verify(repo => repo.InsertStudent(It.Is<Student> (s => s.FirstName == "John" && s.LastName == "Doe")), Times.Once);
            mockStudentRepository.Verify(repo => repo.Save(), Times.Once);
        }

        [Fact]
        public void Create_DbUpdateException_SetsViewBagMessage()
        {
            // Arrange
            var controller = new StudentController(mockStudentRepository.Object);
            var viewModel = new StudentViewModel { FirstName = "John", LastName = "Doe" };
            mockStudentRepository.Setup(repo => repo.InsertStudent(It.IsAny<Student>()))
                .Throws(new DbUpdateException("Test exception", new Exception("Inner exception")));

            // Act
            var result = controller.Create(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("An error occurred while inserting data into the database.", controller.ViewBag.Message);
            Assert.Equal("An error occurred while inserting data into the database.", controller.ModelState[""].Errors[0].ErrorMessage);
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
        public async Task Edit_ModelStateInvalid_ReturnsViewWithViewModel()
        {
            // Arrange 
            var controller = new StudentController(mockStudentRepository.Object);
            controller.ModelState.AddModelError("Error", "Model state is invalid");
            var viewModel = new StudentDetailViewModel();

            // Act
            var result = await controller.Edit(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(viewModel, result.Model);
        }

        [Fact]
        public async Task Edit_StudentNotFound_SetsViewBagMessage()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var controller = new StudentController(mockStudentRepository.Object);
            var viewModel = new StudentDetailViewModel { StudentId = studentId };
            mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId))
                .ReturnsAsync((Student)null);

            // Act
            var result = await controller.Edit(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Student was not found.", controller.ViewBag.Message);
        }

        [Fact]
        public async Task Edit_StudentDataUnchanged_AddsModelErrorAndReturnsView()
        {
            // Arrnage
            var studentId = Guid.NewGuid();
            var controller = new StudentController(mockStudentRepository.Object);
            var viewModel = new StudentDetailViewModel { StudentId = studentId, FirstName = "John", LastName = "Doe" };
            var student = new Student { StudentId = studentId, FirstName = "John", LastName = "Doe" };
            mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);

            // Act
            var result = await controller.Edit(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(viewModel, result.Model);
            Assert.Equal("Data has not been modified", controller.ModelState[string.Empty].Errors[0].ErrorMessage);

        }

        [Fact]
        public async Task Edit_DbUpdateConcurrencyException_HandlesConcurrency()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var controller = new StudentController(mockStudentRepository.Object);
            var viewModel = new StudentDetailViewModel { StudentId = studentId, FirstName = "John", LastName = "Doe" };
            var student = new Student { StudentId = studentId, FirstName = "John", LastName = "Doe", RowVersion = viewModel.RowVersion };
            mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync((Student)student);
            mockStudentRepository.Setup(repo => repo.UpdateStudent(student)).Throws(new DbUpdateConcurrencyException());

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
            var studentId = Guid.NewGuid();
            var controller = new StudentController(mockStudentRepository.Object);
            var viewModel = new StudentDetailViewModel { StudentId = studentId };
            mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ThrowsAsync(new DbUpdateException());

            // Act
            var result = await controller.Edit(viewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("An error occurred while editing data in the database.", controller.ViewBag.Message); ;
        }

        [Fact]
        public async Task Edit_SuccessfulUpdate_RedirectsToIndex()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var mockStudentRepository = new Mock<IStudentRepository>();
            var mockMetadataProvider = new Mock<IModelMetadataProvider>();
            var mockModelBinderFactory = new Mock<IModelBinderFactory>();
            var mockObjectModelValidator = new Mock<IObjectModelValidator>();

            var controller = new StudentController(mockStudentRepository.Object)
            {
                // Injecting necessary context for the controller
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                    ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
                },
                MetadataProvider = mockMetadataProvider.Object,
                ModelBinderFactory = mockModelBinderFactory.Object,
                ObjectValidator = mockObjectModelValidator.Object,

            };

            var viewModel = new StudentDetailViewModel { StudentId = studentId, FirstName = "John", LastName = "Doe" };
            var student = new Student { StudentId = studentId, FirstName = "Johnny", LastName = "Doe" };
            mockStudentRepository.Setup(repo => repo.GetStudentByIdAsync(studentId)).ReturnsAsync(student);
            mockStudentRepository.Setup(repo => repo.UpdateStudent(student)).Verifiable();
            mockStudentRepository.Setup(repo => repo.Save()).Verifiable();

            // Act
            var result = await controller.Edit(viewModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            mockStudentRepository.Verify(repo => repo.UpdateStudent(It.IsAny<Student>()), Times.Once);
            mockStudentRepository.Verify(repo => repo.Save(), Times.Once);
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