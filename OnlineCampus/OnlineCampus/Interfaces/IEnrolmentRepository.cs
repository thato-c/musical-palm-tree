using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface IEnrolmentRepository: IDisposable
    {
        IQueryable<Enrolment> GetEnrolments();
        Task<Enrolment> GetEnrolmentByIdAsync(Guid enrolmentId);
        void InsertEnrolment(Enrolment enrolment);
        Task<Enrolment> DeleteEnrolment(Guid enrolmentId);
        void UpdateEnrolment(Enrolment enrolment);
        void SetOriginalRowVersion(Enrolment enrolment, byte[] rowVersion);
        void Save();
        Task SaveAsync();
    }
}