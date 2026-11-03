using Dapper;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;

namespace LibraryManagement.Repositories.Implementations;

public class CategoryRepository : ICategoryRepository
{
    private const string BookColumns =
        "BookId, Title, ISBN, AuthorId, CategoryId, TotalCopies, AvailableCopies, PublishedYear";

    private readonly IDapperUnitOfWork _uow;

    public CategoryRepository(IDapperUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        var connection = await _uow.GetConnectionAsync();

        var categories = (await connection.QueryAsync<Category>(
            "SELECT CategoryId, CategoryName FROM Categories")).ToList();

        var books = await connection.QueryAsync<Book>(
            $"SELECT {BookColumns} FROM Books");

        var byCategory = categories.ToDictionary(x => x.CategoryId);

        foreach (var book in books)
        {
            if (byCategory.TryGetValue(book.CategoryId, out var category))
            {
                category.Books.Add(book);
            }
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var connection = await _uow.GetConnectionAsync();

        var category = await connection.QueryFirstOrDefaultAsync<Category>(
            "SELECT CategoryId, CategoryName FROM Categories WHERE CategoryId = @id",
            new { id });

        if (category == null)
        {
            return null;
        }

        var books = await connection.QueryAsync<Book>(
            $"SELECT {BookColumns} FROM Books WHERE CategoryId = @id",
            new { id });

        foreach (var book in books)
        {
            category.Books.Add(book);
        }

        return category;
    }

    public async Task AddAsync(Category category)
    {
        _uow.Enqueue(async (connection, tx) =>
        {
            category.CategoryId = await connection.ExecuteScalarAsync<int>(
                "INSERT INTO Categories (CategoryName) VALUES (@CategoryName); "
                + _uow.LastInsertIdQuery,
                category, tx);
        });

        await Task.CompletedTask;
    }

    public void Update(Category category)
    {
        _uow.Enqueue((connection, tx) => connection.ExecuteAsync(
            "UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryId = @CategoryId",
            category, tx));
    }

    public void Delete(Category category)
    {
        _uow.Enqueue((connection, tx) => connection.ExecuteAsync(
            "DELETE FROM Categories WHERE CategoryId = @CategoryId",
            new { category.CategoryId }, tx));
    }

    public async Task SaveChangesAsync()
    {
        await _uow.SaveChangesAsync();
    }
}
