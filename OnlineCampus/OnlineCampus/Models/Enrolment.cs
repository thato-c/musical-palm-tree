using System.ComponentModel.DataAnnotations;

namespace OnlineCampus.Models
{
    public class Enrolment
    {
        public Guid EnrolmentId { get; set; }
        
        public Guid StudentId { get; set; }

        public Guid CourseId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public Student Student { get; set; } = new Student();

        public Course Course { get; set; } = new Course();
    }
}
