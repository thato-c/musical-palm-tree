using System.ComponentModel.DataAnnotations;

namespace OnlineCampus.ViewModels
{
    public class CourseViewModel
    {
        [Required(ErrorMessage = "Course code is required.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course Credits are required.")]
        public int Credits { get; set; } = 0;
    }
}
