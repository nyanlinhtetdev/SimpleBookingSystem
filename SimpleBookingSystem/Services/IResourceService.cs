using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleBookingSystem.ViewModels.Resource;

namespace SimpleBookingSystem.Services
{
    public interface IResourceService
    {
        Task<ResourceViewModel?> GetResourceByIdAsync(Guid resourceId);
        Task<List<SelectListItem>> GetResourceTypeSelectListAsync();
        Task<List<ResourceViewModel>> SearchResourcesAsync(string? keyword, Guid? resourceTypeId);
    }
}