using Microsoft.EntityFrameworkCore;

namespace Backend.Context;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
    {
    }

    public DbSet<Users> Users { get; set; }
}