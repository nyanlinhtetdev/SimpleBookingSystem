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
                    
                    context.Response.Cookies.Delete("AccessToken");
                    context.Response.Cookies.Delete("RefreshToken");
                   
                    context.Response.Redirect("/Account/Login");
                    return; 
                }
            }
        }

        await _next(context);
    }
}