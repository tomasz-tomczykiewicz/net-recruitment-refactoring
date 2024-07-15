using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities;

public class LibraryDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("Library");
    }
}