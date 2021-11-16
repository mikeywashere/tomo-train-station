/// <summary>
/// The operations permitted ont he store based on the readme.md file.
/// </summary>
public interface IDb
{
    Task Delete(string key);
    Task<IEnumerable<string>> Keys();
    Task<byte[]> Fetch(string key);
    Task<int> Set(string key, string body);
}
