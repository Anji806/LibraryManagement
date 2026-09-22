using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repositories.Implementations;

public class BorrowTransactionRepository
    : IBorrowTransactionRepository
{
    private readonly ApplicationDbContext _context;

    public BorrowTransactionRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // Get one transaction by ID
    // Loads:
    // BorrowTransaction -> Book -> Author
    // BorrowTransaction -> Book -> Category
    // BorrowTransaction -> Member
    public async Task<BorrowTransaction?> GetByIdAsync(
        int transactionId)
    {
        return await _context.BorrowTransactions
            .Include(x => x.Book)
                .ThenInclude(x => x.Author)
            .Include(x => x.Book)
                .ThenInclude(x => x.Category)
            .Include(x => x.Member)
            .FirstOrDefaultAsync(
                x => x.TransactionId == transactionId);
    }

    // Check whether a member already has
    // an active borrowing for a particular book
    public async Task<BorrowTransaction?>
        GetActiveBorrowingAsync(
            int bookId,
            int memberId)
    {
        return await _context.BorrowTransactions
            .FirstOrDefaultAsync(x =>
                x.BookId == bookId &&
                x.MemberId == memberId &&
                x.Status == "Borrowed");
    }

    // Get currently borrowed books for a member
    // Loads:
    // BorrowTransaction -> Book -> Author
    // BorrowTransaction -> Book -> Category
    // BorrowTransaction -> Member
    public async Task<List<BorrowTransaction>>
        GetActiveBorrowingsByMemberAsync(
            int memberId)
    {
        return await _context.BorrowTransactions
            .Include(x => x.Book)
                .ThenInclude(x => x.Author)
            .Include(x => x.Book)
                .ThenInclude(x => x.Category)
            .Include(x => x.Member)
            .Where(x =>
                x.MemberId == memberId &&
                x.Status == "Borrowed")
            .AsNoTracking()
            .ToListAsync();
    }

    // Get complete borrowing history for a member
    public async Task<List<BorrowTransaction>>
        GetBorrowHistoryByMemberAsync(
            int memberId)
    {
        return await _context.BorrowTransactions
            .Include(x => x.Book)
                .ThenInclude(x => x.Author)
            .Include(x => x.Book)
                .ThenInclude(x => x.Category)
            .Include(x => x.Member)
            .Where(x => x.MemberId == memberId)
            .OrderByDescending(x => x.BorrowDate)
            .AsNoTracking()
            .ToListAsync();
    }

    // Get complete borrowing history for a book
    public async Task<List<BorrowTransaction>>
        GetBorrowHistoryByBookAsync(
            int bookId)
    {
        return await _context.BorrowTransactions
            .Include(x => x.Book)
                .ThenInclude(x => x.Author)
            .Include(x => x.Book)
                .ThenInclude(x => x.Category)
            .Include(x => x.Member)
            .Where(x => x.BookId == bookId)
            .OrderByDescending(x => x.BorrowDate)
            .AsNoTracking()
            .ToListAsync();
    }

    // Add a new borrowing transaction
    public async Task AddAsync(
        BorrowTransaction transaction)
    {
        await _context.BorrowTransactions
            .AddAsync(transaction);
    }

    // Update an existing borrowing transaction
    public void Update(
        BorrowTransaction transaction)
    {
        _context.BorrowTransactions
            .Update(transaction);
    }

    // Save changes
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}