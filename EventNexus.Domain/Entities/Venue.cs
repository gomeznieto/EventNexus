namespace EventNexus.Domain.Entities;

public class Venue{
   public int Id { get; set; } 
   public string Name { get; set; } = string.Empty;
   public string Adress { get; set; } = string.Empty;
   public double Latitude { get; set; }
   public double Longitude { get; set; }

   public ICollection<Event> Events { get; set; } = [];
}
