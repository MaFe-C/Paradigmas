using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryService.Entities.Models;
using LibraryService.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;

namespace LibraryService.WebAPI.Controllers
{
    [ApiController]
    [Route("api/libraries/{libraryId}/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ILibrariesService _librariesService;
        private readonly IBooksService _booksService;

        public BooksController(IBooksService booksService, ILibrariesService librariesService)
        {
            _librariesService = librariesService;
            _booksService = booksService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(int libraryId)
        {
            var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
            if (library == null)
                return NotFound();

            var books = await _booksService.Get(libraryId, null);
            return Ok(books);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(int libraryId, Book book)
        {
            book.LibraryId = libraryId;

            var created = await _booksService.Add(book);
            if (created == null)
                return NotFound();

            return Created(string.Empty, created);
        }

        [HttpPut("{bookId}")]
        [Authorize]
        public async Task<IActionResult> Update(int libraryId, int bookId, Book book)
        {
            book.Id = bookId;
            book.LibraryId = libraryId;

            var updated = await _booksService.Update(book);
            if (updated == null)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{bookId}")]
        [Authorize]
        public async Task<IActionResult> Delete(int libraryId, int bookId)
        {
            var deleted = await _booksService.Delete(new Book { Id = bookId, LibraryId = libraryId });
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}