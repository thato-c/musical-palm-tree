using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCampus.Models
{
    public class Student
    {
        public Guid StudentId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual User User { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
