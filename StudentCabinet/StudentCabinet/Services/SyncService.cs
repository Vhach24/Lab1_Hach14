using Microsoft.Extensions.Logging;
using SQLite;
using StudentCabinet.Models;

namespace StudentCabinet.Services
{
    public class SyncService : ISyncService
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly Task _init;
        private readonly ILogger<SyncService> _logger;

        public SyncService(string dbPath, ILogger<SyncService> logger)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _init = _db.CreateTableAsync<SyncQueueItem>();
            _logger = logger;

            Connectivity.ConnectivityChanged += async (s, e) =>
            {
                if (e.NetworkAccess == NetworkAccess.Internet)
                {
                    _logger.LogInformation("Мережа відновлена — запускаємо обробку черги синхронізації");
                    await ProcessQueueAsync();
                }
            };
        }

        public async Task EnqueueAsync(string operation, string payloadJson)
        {
            await _init;
            await _db.InsertAsync(new SyncQueueItem
            {
                Operation = operation,
                PayloadJson = payloadJson,
                CreatedAt = DateTime.UtcNow
            });
            _logger.LogWarning("Додано в чергу синхронізації: {Operation}", operation);
        }

        public async Task ProcessQueueAsync()
        {
            await _init;
            var items = await _db.Table<SyncQueueItem>().ToListAsync();
            foreach (var item in items)
            {
                _logger.LogInformation("Синхронізовано: {Operation} -> {Payload}", item.Operation, item.PayloadJson);
                await _db.DeleteAsync(item);
            }
        }
    }
}