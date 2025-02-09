using Microsoft.AspNetCore.Identity;

namespace OnlineCampus.Models
{
    public class User : IdentityUser
    {
        public DateTime DateOfBirth { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Student? Student { get; set; }
        public Admin? Admin { get; set; }
    }
}
