using System.ComponentModel.DataAnnotations;

namespace OnlineCampus.Models
{
    public class Student
    {
        public Guid StudentId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        [Timestamp]
        public byte[] RowVersion { get; set; } = new byte[0];

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
