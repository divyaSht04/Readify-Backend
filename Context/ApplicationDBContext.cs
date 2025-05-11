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
    public DbSet<GlobalDiscount> GlobalDiscounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Accolades)
            .WithOne(a => a.Book)
            .HasForeignKey(a => a.BookID);
        
        
        modelBuilder.Entity<GlobalDiscount>()
            .HasIndex(gd => new { gd.StartDate, gd.EndDate });
    }
}