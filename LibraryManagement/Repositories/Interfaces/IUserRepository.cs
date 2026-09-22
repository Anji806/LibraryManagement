using LibraryManagement.Models;

namespace LibraryManagement.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task AddAsync(User user);

    void Update(User user);

    void Delete(User user);

    Task SaveChangesAsync();
}