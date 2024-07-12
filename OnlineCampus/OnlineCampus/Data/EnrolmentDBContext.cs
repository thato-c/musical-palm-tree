using Microsoft.EntityFrameworkCore;
using OnlineCampus.Models;

namespace OnlineCampus.Data
{
    public class EnrolmentDBContext:DbContext
    {
        public EnrolmentDBContext(DbContextOptions<EnrolmentDBContext> options) : base(options)
        { 
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().ToTable("Student");
            modelBuilder.Entity<Course>().ToTable("Course");
            modelBuilder.Entity<Enrolment>().ToTable("Enrolment");

            // Configure the relationship between Student and Enrolment
            modelBuilder.Entity<Student>()
                .HasMany(student => student.Enrolments)
                .WithOne(enrolment => enrolment.Student)
                .HasForeignKey(enrolment => enrolment.StudentId);

            // Configure the relationship between the Course and Enrolment
            modelBuilder.Entity<Course>()
                .HasMany(course => course.Enrolments)
                .WithOne(enrolment => enrolment.Course)
                .HasForeignKey(enrolment => enrolment.CourseId);
        }

    }
}
