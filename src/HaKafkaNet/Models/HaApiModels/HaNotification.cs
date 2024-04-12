namespace HaKafkaNet;

public record HaNotification
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? UpdateType { get; set; }
}
