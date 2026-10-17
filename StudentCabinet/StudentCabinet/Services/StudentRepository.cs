using System.Text.Json;
using Microsoft.Extensions.Logging;
using SQLite;
using StudentCabinet.Models;

namespace StudentCabinet.Services
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly Task _init;
        private readonly ISyncService _sync;
        private readonly ILogger<StudentRepository> _logger;

        public StudentRepository(string dbPath, ISyncService sync, ILogger<StudentRepository> logger)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _init = _db.CreateTableAsync<Student>();
            _sync = sync;
            _logger = logger;
        }

        public async Task<List<Student>> GetAllAsync()
        {
            await _init;
            var list = await _db.Table<Student>().ToListAsync();
            foreach (var s in list)
                s.FullName = CryptoHelper.Decrypt(s.FullName);
            _logger.LogInformation("Завантажено {Count} студентів", list.Count);
            return list;
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            await _init;
            var s = await _db.FindAsync<Student>(id);
            if (s is not null)
                s.FullName = CryptoHelper.Decrypt(s.FullName);
            return s;
        }

        public async Task<int> SaveAsync(Student student)
        {
            await _init;
            var plainFullName = student.FullName;
            student.FullName = CryptoHelper.Encrypt(plainFullName);

            int result = student.Id == 0
                ? await _db.InsertAsync(student)
                : await _db.UpdateAsync(student);

            student.FullName = plainFullName;
            _logger.LogInformation("Студента {Name} збережено (Id={Id})", plainFullName, student.Id);

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                _logger.LogWarning("Немає мережі — операція Save для {Name} додана в чергу синхронізації", plainFullName);
                await _sync.EnqueueAsync("Save", JsonSerializer.Serialize(student));
            }

            return result;
        }

        public async Task<int> DeleteAsync(Student student)
        {
            await _init;
            var result = await _db.DeleteAsync(student);
            _logger.LogInformation("Студента {Name} видалено (Id={Id})", student.FullName, student.Id);

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                _logger.LogWarning("Немає мережі — операція Delete для {Name} додана в чергу синхронізації", student.FullName);
                await _sync.EnqueueAsync("Delete", JsonSerializer.Serialize(student));
            }

            return result;
        }
    }
}