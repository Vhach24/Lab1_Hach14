using SQLite;
using StudentCabinet.Models;

namespace StudentCabinet.Services
{
    public class SyncService : ISyncService
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly Task _init;

        public SyncService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _init = _db.CreateTableAsync<SyncQueueItem>();

            Connectivity.ConnectivityChanged += async (s, e) =>
            {
                if (e.NetworkAccess == NetworkAccess.Internet)
                    await ProcessQueueAsync();
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
        }

        public async Task ProcessQueueAsync()
        {
            await _init;
            var items = await _db.Table<SyncQueueItem>().ToListAsync();
            foreach (var item in items)
            {
                System.Diagnostics.Debug.WriteLine($"[Sync] {item.Operation} -> {item.PayloadJson}");
                await _db.DeleteAsync(item);
            }
        }
    }
}