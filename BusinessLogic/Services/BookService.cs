using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryService.BusinessLogic.Contracts;
using LibraryService.Entities.Models;

namespace LibraryService.BusinessLogic.Services
{
    public class BooksService : IBooksService
    {
        private readonly IBooksRepository _booksRepository;
        private readonly ILibrariesRepository _librariesRepository;

        public BooksService(IBooksRepository booksRepository, ILibrariesRepository librariesRepository)
        {
            _booksRepository = booksRepository;
            _librariesRepository = librariesRepository;
        }

        public async Task<IEnumerable<Book>> Get(int libraryId, int[]? ids)
            => await _booksRepository.GetAsync(libraryId, ids);

        public async Task<Book?> Add(Book book)
        {
            var libraryExists = (await _librariesRepository.GetAsync(new[] { book.LibraryId })).Any();
            if (!libraryExists)
                return null;

            return await _booksRepository.AddAsync(book);
        }

        public async Task<Book?> Update(Book book)
        {
            var libraryExists = (await _librariesRepository.GetAsync(new[] { book.LibraryId })).Any();
            if (!libraryExists)
                return null;

            return await _booksRepository.UpdateAsync(book);
        }

        public async Task<bool> Delete(Book book)
        {
            var libraryExists = (await _librariesRepository.GetAsync(new[] { book.LibraryId })).Any();
            if (!libraryExists)
                return false;

            return await _booksRepository.DeleteAsync(book.Id, book.LibraryId);
        }
    }

    public interface IBooksService
    {
        Task<IEnumerable<Book>> Get(int libraryId, int[]? ids);

        Task<Book?> Add(Book book);

        Task<Book?> Update(Book book);

        Task<bool> Delete(Book book);
    }
}
