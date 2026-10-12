namespace StudentCabinet.Services
{
    public interface ISyncService
    {
        Task EnqueueAsync(string operation, string payloadJson);
        Task ProcessQueueAsync();
    }
}