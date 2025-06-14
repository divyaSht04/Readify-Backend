using Backend.Model;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Discount = Backend.Model.Discount;


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
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<BannerAnnouncement> BannerAnnouncements { get; set; }
    public DbSet<Whitelist> Whitelists { get; set; }
    public DbSet<PendingRegistration> PendingRegistrations { get; set; }
    
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<BookReview> BookReviews { get; set; }

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

        modelBuilder.Entity<Discount>()
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
            
        // Configure BookReview entity
        modelBuilder.Entity<BookReview>()
            .HasOne(br => br.Book)
            .WithMany()
            .HasForeignKey(br => br.BookId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<BookReview>()
            .HasOne(br => br.User)
            .WithMany()
            .HasForeignKey(br => br.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Ensure a user can only review a book once
        modelBuilder.Entity<BookReview>()
            .HasIndex(br => new { br.UserId, br.BookId })
            .IsUnique();
    }
}