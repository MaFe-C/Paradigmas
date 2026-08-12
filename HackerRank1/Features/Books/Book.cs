using System.ComponentModel.DataAnnotations;
using LibraryService.WebAPI.Features.Libraries;

namespace LibraryService.WebAPI.Features.Books
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Category { get; set; }

        public int LibraryId { get; set; }

        public virtual Library? Library { get; set; }
    }
}