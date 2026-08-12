using Microsoft.EntityFrameworkCore;
using LibraryService.Entities.Models;

namespace LibraryService.DataAccess.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        public DbSet<Library> Libraries { get; set; }

        public DbSet<Book> Books { get; set; }
    }
}