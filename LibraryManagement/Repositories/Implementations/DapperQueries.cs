using Dapper;
using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Implementations;

/// <summary>
/// SQL fragments and mappers shared by the Dapper repositories.
/// </summary>
internal static class DapperQueries
{
    // Book -> Author, Category
    public const string BookWithRelations = @"
SELECT b.BookId, b.Title, b.ISBN, b.AuthorId, b.CategoryId,
       b.TotalCopies, b.AvailableCopies, b.PublishedYear,
       b.PreviewIntroduction, b.PreviewTheme, b.PreviewAbstract, b.PreviewMotto, b.PreviewWhyRead, b.CoverUrl,
       a.AuthorId, a.AuthorName,
       c.CategoryId, c.CategoryName
FROM Books b
INNER JOIN Authors a ON a.AuthorId = b.AuthorId
INNER JOIN Categories c ON c.CategoryId = b.CategoryId";

    public const string BookWithRelationsSplitOn = "AuthorId,CategoryId";

    public static Book MapBook(Book book, Author author, Category category)
    {
        book.Author = author;
        book.Category = category;
        return book;
    }

    // BorrowTransaction -> Book -> Author, Category ; BorrowTransaction -> Member
    public const string TransactionWithRelations = @"
SELECT t.TransactionId, t.BookId, t.MemberId, t.MemberName, t.MemberEmail,
       t.BorrowDate, t.ReturnDate, t.Status,
       b.BookId, b.Title, b.ISBN, b.AuthorId, b.CategoryId,
       b.TotalCopies, b.AvailableCopies, b.PublishedYear,
       a.AuthorId, a.AuthorName,
       c.CategoryId, c.CategoryName,
       u.UserId, u.FullName, u.Email, u.PasswordHash, u.Role, u.IsActive, u.Address
FROM BorrowTransactions t
INNER JOIN Books b ON b.BookId = t.BookId
INNER JOIN Authors a ON a.AuthorId = b.AuthorId
INNER JOIN Categories c ON c.CategoryId = b.CategoryId
LEFT JOIN Users u ON u.UserId = t.MemberId";

    public const string TransactionWithRelationsSplitOn
        = "BookId,AuthorId,CategoryId,UserId";

    public static BorrowTransaction MapTransaction(
        BorrowTransaction transaction,
        Book book,
        Author author,
        Category category,
        User? member)
    {
        book.Author = author;
        book.Category = category;
        transaction.Book = book;
        transaction.Member = member;
        return transaction;
    }

    /// <summary>
    /// Escapes LIKE wildcards so the search text is matched literally,
    /// like EF's string.Contains. Use with ESCAPE '\'.
    /// </summary>
    public static string ContainsPattern(string text)
        => "%" + text
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_")
            .Replace("[", "\\[") + "%";
}
