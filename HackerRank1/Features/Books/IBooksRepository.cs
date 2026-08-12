namespace LibraryService.WebAPI.Features.Books;

public interface IBooksRepository
{
    Task<IEnumerable<Book>> GetAsync(int libraryId, int[]? ids);

    Task<Book> AddAsync(Book book);

    Task<Book> UpdateAsync(Book book);

    Task<bool> DeleteAsync(int id, int libraryId);
}
