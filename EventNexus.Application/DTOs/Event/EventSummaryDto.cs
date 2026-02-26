namespace EventNexus.Application.DTOs;

public class EventSummaryDto{
    public string Title { get; set; } = string.Empty;
    public string? UrlImage { get; set; }
    public DateTime StartDate { get; set; }
    public int AvailableTickets { get; set; }
}
