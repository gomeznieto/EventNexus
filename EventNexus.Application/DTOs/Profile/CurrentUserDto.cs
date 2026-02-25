namespace EventNexus.Application.DTOs;

public class CurrentUserDto {
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public IList<string> Roles { get; set; } = []; 
    
    public string? ComapanyName { get; set; }
    public string? BusinessPhone { get; set; }
}
