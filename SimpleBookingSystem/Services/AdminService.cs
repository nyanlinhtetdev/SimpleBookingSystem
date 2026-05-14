using BookingSystem.Database.AppDbContextModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleBookingSystem.ViewModels.Admin;

namespace SimpleBookingSystem.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    public async Task<AdminDashboardViewModel> GetDashboardDataAsync()
    {
        var recentBookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Resource)
                .ThenInclude(r => r.ResourceType)
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(b => new AdminBookingViewModel
            {
                BookingId = b.BookingId,
                UserName = b.User.FullName,
                UserEmail = b.User.Email,
                ResourceName = b.Resource.Name,
                TypeName = b.Resource.ResourceType.TypeName,
                Location = b.Resource.Location,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();

        return new AdminDashboardViewModel
        {
            TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted),
            TotalResources = await _context.Resources.CountAsync(r => !r.IsDeleted),
            TotalBookings = await _context.Bookings.CountAsync(),
            ActiveBookings = await _context.Bookings.CountAsync(b => b.Status == "Active"),
            CancelledBookings = await _context.Bookings.CountAsync(b => b.Status == "Cancelled"),
            RecentBookings = recentBookings
        };
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    public async Task<List<AdminBookingViewModel>> GetAllBookingsAsync()
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Resource)
                .ThenInclude(r => r.ResourceType)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new AdminBookingViewModel
            {
                BookingId = b.BookingId,
                UserName = b.User.FullName,
                UserEmail = b.User.Email,
                ResourceName = b.Resource.Name,
                TypeName = b.Resource.ResourceType.TypeName,
                Location = b.Resource.Location,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> CancelAnyBookingAsync(Guid bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.Status == "Active");

        if (booking == null) return false;

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    public async Task<List<AdminUserViewModel>> GetAllUsersAsync()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserViewModel
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt,
                TotalBookings = u.Bookings.Count
            })
            .ToListAsync();
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user == null) return false;

        user.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactivateUserAsync(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user == null) return false;

        user.IsActive = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteUserAsync(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user == null) return false;

        // Also revoke all refresh tokens for this user
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();

        tokens.ForEach(t => t.IsRevoked = true);

        user.IsDeleted = true;
        user.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Resources ─────────────────────────────────────────────────────────────

    public async Task<List<AdminResourceViewModel>> GetAllResourcesAsync()
    {
        return await _context.Resources
            .Include(r => r.ResourceType)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.ResourceType.TypeName)
            .ThenBy(r => r.Name)
            .Select(r => new AdminResourceViewModel
            {
                ResourceId = r.ResourceId,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location,
                TypeName = r.ResourceType.TypeName,
                ResourceTypeId = r.ResourceTypeId,
                IsActive = r.IsActive,
                IsDeleted = r.IsDeleted,
                TotalBookings = r.Bookings.Count
            })
            .ToListAsync();
    }

    public async Task<bool> CreateResourceAsync(ResourceFormViewModel model)
    {
        var resource = new Resource
        {
            Name = model.Name,
            Description = model.Description,
            Location = model.Location,
            ResourceTypeId = model.ResourceTypeId,
            IsActive = model.IsActive,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ResourceFormViewModel?> GetResourceFormAsync(Guid resourceId)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.ResourceId == resourceId && !r.IsDeleted);

        if (resource == null) return null;

        return new ResourceFormViewModel
        {
            ResourceId = resource.ResourceId,
            Name = resource.Name,
            Description = resource.Description,
            Location = resource.Location,
            ResourceTypeId = resource.ResourceTypeId,
            IsActive = resource.IsActive,
            ResourceTypes = await GetResourceTypeSelectListAsync()
        };
    }

    public async Task<bool> EditResourceAsync(ResourceFormViewModel model)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.ResourceId == model.ResourceId && !r.IsDeleted);

        if (resource == null) return false;

        resource.Name = model.Name;
        resource.Description = model.Description;
        resource.Location = model.Location;
        resource.ResourceTypeId = model.ResourceTypeId;
        resource.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleResourceActiveAsync(Guid resourceId)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.ResourceId == resourceId && !r.IsDeleted);

        if (resource == null) return false;

        resource.IsActive = !resource.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteResourceAsync(Guid resourceId)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.ResourceId == resourceId && !r.IsDeleted);

        if (resource == null) return false;

        resource.IsDeleted = true;
        resource.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private async Task<List<SelectListItem>> GetResourceTypeSelectListAsync()
    {
        return await _context.ResourceTypes
            .Where(rt => !rt.IsDeleted)
            .OrderBy(rt => rt.TypeName)
            .Select(rt => new SelectListItem
            {
                Value = rt.ResourceTypeId.ToString(),
                Text = rt.TypeName
            })
            .ToListAsync();
    }
}