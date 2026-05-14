using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SimpleBookingSystem.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SimpleBookingSystem.Middlewares;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRefreshMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService, IConfiguration config)
    {
        var accessToken = context.Request.Cookies["AccessToken"];
        var refreshToken = context.Request.Cookies["RefreshToken"];

        // Only attempt refresh if access token is missing/expired but refresh token exists
        if (string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
        {
            await TryRefreshTokenAsync(context, authService, config, refreshToken);
        }
        else if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
        {
            // Access token exists — check if it's expired
            if (IsTokenExpired(accessToken, config))
            {
                await TryRefreshTokenAsync(context, authService, config, refreshToken);
            }
        }

        await _next(context);
    }

    private async Task TryRefreshTokenAsync(
        HttpContext context,
        IAuthService authService,
        IConfiguration config,
        string refreshToken)
    {
        var stored = await authService.GetRefreshTokenAsync(refreshToken);

        // Refresh token is invalid, revoked, or expired — do nothing, let OnChallenge handle redirect
        if (stored == null || stored.IsRevoked || stored.ExpiryDate < DateTime.UtcNow)
            return;

        // ✅ Refresh token is valid — rotate tokens silently
        var newAccessToken = authService.GenerateAccessToken(stored.User);
        var newRefreshToken = authService.GenerateRefreshToken();

        // Revoke old refresh token
        await authService.RevokeRefreshTokenAsync(refreshToken);

        // Save new refresh token to DB
        await authService.SaveRefreshTokenAsync(stored.UserId, newRefreshToken);

        // Issue new cookies — user won't notice anything happened
        context.Response.Cookies.Append("AccessToken", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        context.Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        // Inject the new access token into the current request
        // so JwtBearer can authenticate this request immediately
        context.Request.Headers["Authorization"] = $"Bearer {newAccessToken}";
    }

    private bool IsTokenExpired(string token, IConfiguration config)
    {
        try
        {
            var jwtSettings = config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(
                                   Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,  // This is what checks expiry
                ClockSkew = TimeSpan.Zero  // No tolerance — exact expiry
            }, out _);

            return false; // Token is still valid
        }
        catch (SecurityTokenExpiredException)
        {
            return true;  // Token is expired — needs refresh
        }
        catch
        {
            return false; // Other errors (tampered token etc.) — let JwtBearer handle it
        }
    }
}