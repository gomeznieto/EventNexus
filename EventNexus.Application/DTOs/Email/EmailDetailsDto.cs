namespace EventNexus.Application.DTOs;

public class EmailDetailsDto{
    public string Destination { get; set; } = string.Empty;
    public string Subject  { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
