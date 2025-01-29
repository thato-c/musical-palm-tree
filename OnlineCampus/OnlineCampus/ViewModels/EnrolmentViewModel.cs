using OnlineCampus.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCampus.ViewModels
{
    public class EnrolmentViewModel
    {
        [Required(ErrorMessage = "StudentId is required")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "CourseId is required")]
        public Guid CourseId { get; set; }
    }
}
