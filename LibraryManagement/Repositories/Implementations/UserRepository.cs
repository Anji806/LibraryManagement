using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.UserId == id);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task DeleteKeepingHistoryAsync(User user)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        // Snapshot who borrowed before the link to the user is cut.
        await _context.BorrowTransactions
            .Where(x => x.MemberId == user.UserId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.MemberName, user.FullName)
                .SetProperty(x => x.MemberEmail, user.Email));

        // The FK is ON DELETE SET NULL, so the history rows survive.
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        await tx.CommitAsync();
    }

    /// <summary>
    /// BorrowingCount — transactions not yet returned (ReturnDate IS NULL).
    /// ReturnCount    — transactions already returned (ReturnDate IS NOT NULL).
    /// </summary>
    public async Task<(int BorrowingCount, int ReturnCount)>
        GetTransactionCountsAsync(int userId)
    {
        var borrowingCount = await _context.BorrowTransactions
            .CountAsync(x => x.MemberId == userId && x.ReturnDate == null);

        var returnCount = await _context.BorrowTransactions
            .CountAsync(x => x.MemberId == userId && x.ReturnDate != null);

        return (borrowingCount, returnCount);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}