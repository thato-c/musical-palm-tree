namespace OnlineCampus.Models
{
    public class Course
    {
        public Guid CourseId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Credits { get; set; } = 0;

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
