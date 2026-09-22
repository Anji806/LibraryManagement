using LibraryManagement.DTOs.Auth;
using LibraryManagement.Exceptions;
using LibraryManagement.Helpers;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using LibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Services.Implementations;

public class AuthService : IAuthService

{
    private readonly IAuthRepository _authRepository;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        IAuthRepository authRepository,
        JwtTokenGenerator jwtTokenGenerator)
    {
        _authRepository = authRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterDto dto)
    {
        var existingUser =
            await _authRepository.GetByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            throw new BusinessRuleException(
                "An account with this email already exists.");
        }

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Role = "Member",
            IsActive = true
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                dto.Password);

        await _authRepository.AddAsync(user);
        await _authRepository.SaveChangesAsync();

        var token =
            _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginDto dto)
    {
        var user =
            await _authRepository.GetByEmailAsync(dto.Email);

        if (user == null)
        {
            throw new BusinessRuleException(
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleException(
                "This account is inactive.");
        }

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

        if (passwordResult ==
            PasswordVerificationResult.Failed)
        {
            throw new BusinessRuleException(
                "Invalid email or password.");
        }

        var token =
            _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}