using SimpleBookingSystem.ViewModels.Admin;

namespace SimpleBookingSystem.Services;

public interface IAdminService
{
    // Dashboard
    Task<AdminDashboardViewModel> GetDashboardDataAsync();

    // Bookings — paginated
    Task<PagedResult<AdminBookingViewModel>> GetAllBookingsAsync(int page, int pageSize);
    Task<bool> CancelAnyBookingAsync(Guid bookingId);

    // Users — paginated
    Task<PagedResult<AdminUserViewModel>> GetAllUsersAsync(int page, int pageSize);
    Task<bool> DeactivateUserAsync(Guid userId);
    Task<bool> ReactivateUserAsync(Guid userId);
    Task<bool> SoftDeleteUserAsync(Guid userId);

    // Resources
    Task<List<AdminResourceViewModel>> GetAllResourcesAsync();
    Task<bool> CreateResourceAsync(ResourceFormViewModel model);
    Task<ResourceFormViewModel?> GetResourceFormAsync(Guid resourceId);
    Task<bool> EditResourceAsync(ResourceFormViewModel model);
    Task<bool> ToggleResourceActiveAsync(Guid resourceId);
    Task<bool> SoftDeleteResourceAsync(Guid resourceId);
}