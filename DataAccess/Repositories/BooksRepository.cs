using LibraryService.BusinessLogic.Contracts;
using LibraryService.DataAccess.Data;
using LibraryService.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.DataAccess.Repositories;

public class BooksRepository : IBooksRepository
{
    private readonly LibraryContext _context;

    public BooksRepository(LibraryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAsync(int libraryId, int[]? ids)
    {
        var query = _context.Books.AsQueryable().Where(b => b.LibraryId == libraryId);

        if (ids != null && ids.Any())
            query = query.Where(b => ids.Contains(b.Id));

        return await query.ToListAsync();
    }

    public async Task<Book> AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);

        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book> UpdateAsync(Book book)
    {
        var existing = await _context.Books.SingleAsync(x => x.Id == book.Id && x.LibraryId == book.LibraryId);
        existing.Name = book.Name;
        existing.Category = book.Category;

        _context.Books.Update(existing);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<bool> DeleteAsync(int id, int libraryId)
    {
        var book = await _context.Books.SingleOrDefaultAsync(x => x.Id == id && x.LibraryId == libraryId);
        if (book == null)
            return false;

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }
}
