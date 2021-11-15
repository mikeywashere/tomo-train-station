
namespace TrainSchedule.Database
{
    public interface IDb
    {
        Task Delete(string key);
        Task<IEnumerable<string>> GetKeys();
        Task<byte[]> Read(string key);
        Task<int> Write(string key, string body);
    }
}