using LibraryManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly IBorrowTransactionRepository
        _borrowRepository;

    public MembersController(
        IBorrowTransactionRepository borrowRepository)
    {
        _borrowRepository = borrowRepository;
    }

    // GET: api/members/1/borrowed-books
    [HttpGet("{memberId:int}/borrowed-books")]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public async Task<IActionResult> GetBorrowedBooks(
        int memberId)
    {
        var transactions =
            await _borrowRepository
                .GetActiveBorrowingsByMemberAsync(memberId);

        return Ok(transactions);
    }

    // GET: api/members/1/borrow-history
    [HttpGet("{memberId:int}/borrow-history")]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public async Task<IActionResult> GetBorrowHistory(
        int memberId)
    {
        var transactions =
            await _borrowRepository
                .GetBorrowHistoryByMemberAsync(memberId);

        return Ok(transactions);
    }
}