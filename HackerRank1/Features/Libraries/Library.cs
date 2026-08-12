using System.ComponentModel.DataAnnotations;

namespace LibraryService.WebAPI.Features.Libraries
{
    public class Library
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }
    }
}