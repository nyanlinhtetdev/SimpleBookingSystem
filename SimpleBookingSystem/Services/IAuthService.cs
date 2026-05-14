using BookingSystem.Database.AppDbContextModels;

namespace SimpleBookingSystem.Services
{
    public interface IAuthService
    {
        // Generates a short-lived JWT access token containing user claims
        string GenerateAccessToken(User user);

        // Generates a cryptographically secure random refresh token string
        string GenerateRefreshToken();

        // Saves a new refresh token record to the database
        Task SaveRefreshTokenAsync(Guid userId, string token);

        // Retrieves a refresh token record from the database by token string
        Task<RefreshToken?> GetRefreshTokenAsync(string token);

        // Marks a refresh token as revoked (used on logout or rotation)
        Task RevokeRefreshTokenAsync(string token);
    }
}
