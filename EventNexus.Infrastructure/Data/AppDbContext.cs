using EventNexus.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace EventNexus.Infrastructure.Data;

public class AppDbContext : IdentityDbContext {
    public AppDbContext(DbContextOptions<AppDbContext> option) : base (option){}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>()
            .Property(t => t.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(t => t.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Event>()
            .Property( e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Event>()
            .Property( e => e.Modality)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .HasOne(t => t.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }
    public DbSet<Organizer> Organizers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Order> Orders { get; set; }
}
