using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineCampus.Models;

namespace OnlineCampus.Data
{
    public class EnrolmentDBContext:IdentityDbContext<User>
    {
        public EnrolmentDBContext(DbContextOptions<EnrolmentDBContext> options) : base(options)
        { 
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>().ToTable("Student");
            modelBuilder.Entity<Course>().ToTable("Course");
            modelBuilder.Entity<Enrolment>().ToTable("Enrolment");
            modelBuilder.Entity<Admin>().ToTable("Admin");

            // One-to-One Relationship
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-One Relationship
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
