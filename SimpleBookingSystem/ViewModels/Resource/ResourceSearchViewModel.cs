using Microsoft.AspNetCore.Mvc.Rendering;

namespace SimpleBookingSystem.ViewModels.Resource
{
    public class ResourceSearchViewModel
    {
        public string? Keyword { get; set; }
        public Guid? ResourceTypeId { get; set; }

        // Search results
        public List<ResourceViewModel> Resources { get; set; } = new();

        // Dropdown options — populated from DB
        public List<SelectListItem> ResourceTypes { get; set; } = new();
    }
}
