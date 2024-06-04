using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineCampus.Controllers;
using OnlineCampus.Interfaces;

namespace OnlineCampus.Tests
{
    public class StudentControllerTest
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
    }
}