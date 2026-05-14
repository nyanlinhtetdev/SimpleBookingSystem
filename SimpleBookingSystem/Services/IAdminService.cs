using SimpleBookingSystem.ViewModels.Admin;

namespace SimpleBookingSystem.Services
{
    public interface IAdminService
    {
        Task<bool> CancelAnyBookingAsync(Guid bookingId);
        Task<bool> CreateResourceAsync(ResourceFormViewModel model);
        Task<bool> DeactivateUserAsync(Guid userId);
        Task<bool> EditResourceAsync(ResourceFormViewModel model);
        Task<List<AdminBookingViewModel>> GetAllBookingsAsync();
        Task<List<AdminResourceViewModel>> GetAllResourcesAsync();
        Task<List<AdminUserViewModel>> GetAllUsersAsync();
        Task<AdminDashboardViewModel> GetDashboardDataAsync();
        Task<ResourceFormViewModel?> GetResourceFormAsync(Guid resourceId);
        Task<bool> ReactivateUserAsync(Guid userId);
        Task<bool> SoftDeleteResourceAsync(Guid resourceId);
        Task<bool> SoftDeleteUserAsync(Guid userId);
        Task<bool> ToggleResourceActiveAsync(Guid resourceId);
    }
}