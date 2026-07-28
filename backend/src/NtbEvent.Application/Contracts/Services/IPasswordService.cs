using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Services;

public interface IPasswordService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string hashedPassword, string providedPassword);
}
