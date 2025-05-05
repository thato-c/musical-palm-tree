using Microsoft.EntityFrameworkCore;
using OnlineCampus.DTOs;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;

namespace OnlineCampus.Services
{
    public class StudentService: IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<OperationResultWithData<List<Student>>> GetAllStudentsAsync()
        {
            var students = await _studentRepository.GetStudents().ToListAsync();

            if (students == null)
            {
                return new OperationResultWithData<List<Student>> { Success = false, Message = "Student not found" };
            }
            return new OperationResultWithData<List<Student>> { Success = true, Data = students };
        }

        public async Task<OperationResultWithData<Student>> GetStudentByIdAsync(Guid studentId)
        {
            var student = await _studentRepository.GetStudentByIdAsync(studentId);

            if (student == null)
            {
                return new OperationResultWithData<Student> { Success = false, Message = "Student not found" };
            }
            return new OperationResultWithData<Student> {Success = true, Data = student };
        }

        public async Task<OperationResultWithData<Student>> GetStudentByIdWithEnrolledCoursesAsync(Guid studentId)
        {
            var student = await _studentRepository.GetStudentWithCoursesByIdAsync(studentId);

            if (student == null)
            {
                return new OperationResultWithData<Student> { Success = false, Message = "Student not found" };
            }
            return new OperationResultWithData<Student> { Success = true, Data = student };
        }

        public async Task<OperationResult> CreateStudentAsync(Student student)
        {
            _studentRepository.InsertStudent(student);
            await _studentRepository.SaveAsync();
            return new OperationResult { Success = true };
        }

        public async Task<OperationResult> UpdateStudentAsync(Student student)
        {
            _studentRepository.SetOriginalRowVersion(student, student.RowVersion);
            _studentRepository.UpdateStudent(student);
            await _studentRepository.SaveAsync();
            return new OperationResult { Success = true };
        }

        public async Task<OperationResult> DeleteStudentAsync(Guid studentId)
        {
            await _studentRepository.DeleteStudent(studentId);
            await _studentRepository.SaveAsync();
            return new OperationResult { Success = true };
        }
    }
}
