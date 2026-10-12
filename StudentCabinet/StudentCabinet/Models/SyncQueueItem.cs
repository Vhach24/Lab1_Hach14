using SQLite;

namespace StudentCabinet.Models
{
    public class SyncQueueItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string EntityType { get; set; } = "Student";

        public string Operation { get; set; } = string.Empty; // Add, Update, Delete

        public string PayloadJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}