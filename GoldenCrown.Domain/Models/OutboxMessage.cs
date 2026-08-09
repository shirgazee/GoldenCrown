namespace GoldenCrown.Domain.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; }
    
    public string Payload { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? SentAt { get; set; }
    
    public int Attempts { get; set; }
    
    public string? Error { get; set; }
}