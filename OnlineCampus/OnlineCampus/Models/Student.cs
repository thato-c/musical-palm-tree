namespace OnlineCampus.Models
{
    public class Student
    {
        public Guid StudentId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
