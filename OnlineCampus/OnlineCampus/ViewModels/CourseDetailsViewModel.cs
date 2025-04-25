using OnlineCampus.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCampus.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Guid CourseId { get; set; }

        [Required(ErrorMessage = "Course code is required.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Credits are required.")]
        public int Credits { get; set; } = 0;

        public List<Student> EnrolledStudents { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
