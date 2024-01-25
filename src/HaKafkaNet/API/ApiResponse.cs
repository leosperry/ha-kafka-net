namespace HaKafkaNet;

public record ApiResponse<Tdata>
{
    public required Tdata Data { get; init; }}

public record ApiResponse<Tmeta, Tdata> : ApiResponse<Tdata>
{
    public Tmeta? Meta { get; init; } 
}
