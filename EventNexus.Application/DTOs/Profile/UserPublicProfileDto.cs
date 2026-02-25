namespace EventNexus.Application.DTOs;

public class UserPublicProfileDto{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; } = string.Empty;

    public string? CompanyName { get; set; }

    public List<DTOs.EventSummaryDto> HostedEvents { get; set; } = [];

}
