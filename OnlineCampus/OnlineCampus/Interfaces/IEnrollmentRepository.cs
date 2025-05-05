using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface IEnrollmentRepository: IDisposable
    {
        IQueryable<Enrolment> GetEnrollments();
        Task<Enrolment> GetEnrollmentByIdAsync(Guid enrolmentId);
        void InsertEnrollment(Enrolment enrolment);
        Task<Enrolment> DeleteEnrollment(Guid enrolmentId);
        void UpdateEnrollment(Enrolment enrolment);
        void SetOriginalRowVersion(Enrolment enrolment, byte[] rowVersion);
        void Save();
        Task SaveAsync();
    }
}