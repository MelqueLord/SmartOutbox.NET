namespace SmartOutbox.Core.Interfaces
{
    public interface IJsonSerializerService
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string payload);
    }
}
