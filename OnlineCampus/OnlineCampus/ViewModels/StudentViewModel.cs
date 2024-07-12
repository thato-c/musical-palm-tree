using System.ComponentModel.DataAnnotations;

namespace OnlineCampus.ViewModels
{
    public class StudentViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;
    }
}
