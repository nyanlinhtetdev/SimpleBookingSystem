using BookingSystem.Database.AppDbContextModels;

namespace SimpleBookingSystem.Services
{
    public interface IAuthService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();

        Task SaveRefreshTokenAsync(Guid userId, string token);

        Task<RefreshToken?> GetRefreshTokenAsync(string token);

        Task RevokeRefreshTokenAsync(string token);
    }
}
