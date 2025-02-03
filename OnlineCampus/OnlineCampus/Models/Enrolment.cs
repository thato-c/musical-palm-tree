using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCampus.Models
{
    public class Enrolment
    {
        public Guid EnrolmentId { get; set; }
        
        public Guid StudentId { get; set; }

        public Guid CourseId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}
