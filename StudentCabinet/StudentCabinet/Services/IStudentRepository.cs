using StudentCabinet.Models;

namespace StudentCabinet.Services
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAllAsync();
        Task<Student?> GetByIdAsync(int id);
        Task<int> SaveAsync(Student student);
        Task<int> DeleteAsync(Student student);
    }
}