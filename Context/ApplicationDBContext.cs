using Backend.Model;
using Backend.Models;
using Microsoft.EntityFrameworkCore;


namespace Backend.Context;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
    {
    }

    public DbSet<Users> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookAccolade> BookAccolades { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<GlobalDiscount> GlobalDiscounts { get; set; }
    public DbSet<BannerAnnouncement> BannerAnnouncements { get; set; }
    public DbSet<Whitelist> Whitelists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Accolades)
            .WithOne(a => a.Book)
            .HasForeignKey(a => a.BookID);
            
        modelBuilder.Entity<Cart>()
           .HasOne(c => c.User)
           .WithMany()
           .HasForeignKey(c => c.UserId)
           .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<CartItem>()
            .HasKey(ci => new { ci.BookId, ci.CartId });
        
            
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Book)
            .WithMany()
            .HasForeignKey(ci => ci.BookId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<GlobalDiscount>()
            .HasIndex(gd => new { gd.StartDate, gd.EndDate });

        modelBuilder.Entity<Whitelist>()
            .HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Whitelist>()
            .HasOne(w => w.Book)
            .WithMany()
            .HasForeignKey(w => w.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Whitelist>()
            .HasIndex(w => new { w.UserId, w.BookId })
            .IsUnique();

        // Configure BookId and UserId as Guid in Whitelist
        modelBuilder.Entity<Whitelist>()
            .Property(w => w.BookId)
            .HasColumnType("uuid");
        modelBuilder.Entity<Whitelist>()
            .Property(w => w.UserId)
            .HasColumnType("uuid");
    }
}