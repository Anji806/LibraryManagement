using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class BorrowTransaction
{
    [Key]
    public int TransactionId { get; set; }

    public int BookId { get; set; }

    public int MemberId { get; set; }

    public DateTime BorrowDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public string Status { get; set; } = "Borrowed";

    public Book Book { get; set; } = null!;

    public User Member { get; set; } = null!;
}