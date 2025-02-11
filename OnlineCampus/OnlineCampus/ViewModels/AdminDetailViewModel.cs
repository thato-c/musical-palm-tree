using System.ComponentModel.DataAnnotations;

namespace OnlineCampus.ViewModels
{
    public class AdminDetailViewModel
    {
        public Guid AdminId { get; set; }

        [Required(ErrorMessage ="First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage ="Last Name is required")]
        public string LastName { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
