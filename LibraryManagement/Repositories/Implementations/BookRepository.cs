using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories.Implementations;

public class BookRepository : IBookRepository
{
    private readonly ApplicationDbContext _context;

    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books
            .Include(x => x.Author)
            .Include(x => x.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books
            .Include(x => x.Author)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.BookId == id);
    }

    public async Task<List<Book>> SearchAsync(string title)
    {
        return await _context.Books
            .Include(x => x.Author)
            .Include(x => x.Category)
            .Where(x => x.Title.Contains(title))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
    }

    public void Update(Book book)
    {
        _context.Books.Update(book);
    }

    public void Delete(Book book)
    {
        _context.Books.Remove(book);
    }

    public async Task<bool> HasActiveBorrowingsAsync(int bookId)
    {
        return await _context.BorrowTransactions
            .AnyAsync(x =>
                x.BookId == bookId &&
                x.Status == "Borrowed");
    }

    public async Task<bool> ExistsAsync(int bookId)
    {
        return await _context.Books
            .AnyAsync(x => x.BookId == bookId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
