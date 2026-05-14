using BookingSystem.Database.AppDbContextModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleBookingSystem.Services;
using SimpleBookingSystem.ViewModels.Account;

namespace SimpleBookingSystem.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public AccountController(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Check if email is already taken
        bool emailExists = await _context.Users
                               .AnyAsync(u => u.Email == model.Email && !u.IsDeleted);
        if (emailExists)
        {
            ModelState.AddModelError("Email", "This email is already registered.");
            return View(model);
        }

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            // Hash the plain password before storing — never store plain text
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = "User",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Find user by email (exclude soft deleted users)
        var user = await _context.Users
                       .FirstOrDefaultAsync(u => u.Email == model.Email && !u.IsDeleted);

        // Verify plain password against stored hash using BCrypt
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // Block deactivated users from logging in
        if (!user.IsActive)
        {
            ModelState.AddModelError("", "Your account has been deactivated. Please contact admin.");
            return View(model);
        }

        // Generate tokens
        var accessToken = _authService.GenerateAccessToken(user);
        var refreshToken = _authService.GenerateRefreshToken();

        // Save refresh token to database
        await _authService.SaveRefreshTokenAsync(user.UserId, refreshToken);

        // Store access token in HttpOnly cookie (not accessible by JavaScript)
        Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        // Store refresh token in HttpOnly cookie
        Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        // ⭐ Redirect to returnUrl if provided and safe, otherwise redirect by role
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return user.Role == "Admin"
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("Index", "Resource");
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["RefreshToken"];

        // Revoke refresh token in database so it can't be reused
        if (refreshToken != null)
            await _authService.RevokeRefreshTokenAsync(refreshToken);

        // Delete both cookies from the browser
        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken");

        return RedirectToAction("Login");
    }

    // ── Refresh Token ─────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> RefreshToken()
    {
        var oldRefreshToken = Request.Cookies["RefreshToken"];

        if (oldRefreshToken == null)
            return RedirectToAction("Login");

        var stored = await _authService.GetRefreshTokenAsync(oldRefreshToken);

        // Validate: must exist, not revoked, and not expired
        if (stored == null || stored.IsRevoked || stored.ExpiryDate < DateTime.UtcNow)
        {
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
            return RedirectToAction("Login");
        }

        // Generate new tokens (rotation — old refresh token is revoked)
        var newAccessToken = _authService.GenerateAccessToken(stored.User);
        var newRefreshToken = _authService.GenerateRefreshToken();

        // Revoke old refresh token
        await _authService.RevokeRefreshTokenAsync(oldRefreshToken);

        // Save new refresh token to database
        await _authService.SaveRefreshTokenAsync(stored.UserId, newRefreshToken);

        // Issue new cookies
        Response.Cookies.Append("AccessToken", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok();
    }
}