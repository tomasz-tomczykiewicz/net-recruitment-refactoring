using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities;

public class LibraryDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }
}