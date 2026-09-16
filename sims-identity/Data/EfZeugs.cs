using Microsoft.EntityFrameworkCore;

namespace sims_identity.Data;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<User> User { get; set; }
}