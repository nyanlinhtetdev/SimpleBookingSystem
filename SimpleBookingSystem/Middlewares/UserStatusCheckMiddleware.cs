using BookingSystem.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SimpleBookingSystem.Middlewares;

public class UserStatusCheckMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        // Only check if the user is currently logged in
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                // Unpack the bare minimum data to make this query super fast
                var isUserActive = await dbContext.Users
                    .Where(u => u.UserId == userId)
                    .Select(u => u.IsActive && !u.IsDeleted)
                    .FirstOrDefaultAsync();

                if (!isUserActive)
                {
                    // User was deactivated/deleted while logged in! 
                    // Nuke their cookies and force logout.
                    context.Response.Cookies.Delete("AccessToken");
                    context.Response.Cookies.Delete("RefreshToken");

                    // Optional: Add a message to display on the login screen
                    // context.Session or TempData is normally used, but since we are in middleware,
                    // a simple redirect with a query param works well.
                    context.Response.Redirect("/Account/Login");
                    return; // Stop processing this request
                }
            }
        }

        await _next(context);
    }
}