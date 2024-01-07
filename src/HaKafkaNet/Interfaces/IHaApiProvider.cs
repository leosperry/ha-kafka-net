namespace HaKafkaNet;

public interface IHaApiProvider
{
    Task PersistentNotification(string message);
    Task CallService(string domain, string service, object data);
}
