
using LibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BorrowTransactionsController : ControllerBase
{
    private readonly IBorrowService _borrowService;

    public BorrowTransactionsController(
        IBorrowService borrowService)
    {
        _borrowService = borrowService;
    }

    // POST: api/borrowtransactions/borrow
    [HttpPost("borrow")]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public async Task<IActionResult> BorrowBook(
        int bookId,
        int memberId)
    {
        var transaction =
            await _borrowService.BorrowAsync(
                bookId,
                memberId);

        return Ok(new
        {
            message = "Book borrowed successfully.",
            transaction
        });
    }

    // PUT: api/borrowtransactions/return/1
    [HttpPut("return/{transactionId:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> ReturnBook(
        int transactionId)
    {
        var transaction =
            await _borrowService.ReturnAsync(
                transactionId);

        return Ok(new
        {
            message = "Book returned successfully.",
            transaction
        });
    }
}