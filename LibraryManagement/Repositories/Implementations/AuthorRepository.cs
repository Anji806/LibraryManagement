using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories.Implementations;

public class AuthorRepository : IAuthorRepository
{
    private readonly ApplicationDbContext _context;

    public AuthorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Author>> GetAllAsync()
    {
        return await _context.Authors
            .Include(x => x.Books)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Author?> GetByIdAsync(int id)
    {
        return await _context.Authors
            .Include(x => x.Books)
            .FirstOrDefaultAsync(
                x => x.AuthorId == id);
    }

    public async Task AddAsync(Author author)
    {
        await _context.Authors.AddAsync(author);
    }

    public void Update(Author author)
    {
        _context.Authors.Update(author);
    }

    public void Delete(Author author)
    {
        _context.Authors.Remove(author);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}