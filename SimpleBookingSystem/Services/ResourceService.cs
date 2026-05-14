using BookingSystem.Database.AppDbContextModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleBookingSystem.ViewModels.Resource;
using System.ComponentModel.Design;

namespace SimpleBookingSystem.Services;

public class ResourceService : IResourceService
{
    private readonly AppDbContext _context;

    public ResourceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResourceViewModel>> SearchResourcesAsync(string? keyword, Guid? resourceTypeId)
    {
        // Start with base query — only active, non-deleted resources
        var query = _context.Resources
            .Include(r => r.ResourceType)
            .Where(r => !r.IsDeleted && r.IsActive);

        // Filter by keyword if provided — searches resource name using LIKE
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{keyword}%"));

        // Filter by resource type if selected
        if (resourceTypeId.HasValue)
            query = query.Where(r => r.ResourceTypeId == resourceTypeId.Value);

        return await query
            .OrderBy(r => r.ResourceType.TypeName)
            .ThenBy(r => r.Name)
            .Select(r => new ResourceViewModel
            {
                ResourceId = r.ResourceId,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location,
                TypeName = r.ResourceType.TypeName,
                ResourceTypeId = r.ResourceTypeId,
                IsActive = r.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetResourceTypeSelectListAsync()
    {
        // Pull distinct active types from DB for the dropdown
        var types = await _context.ResourceTypes
            .Where(rt => !rt.IsDeleted)
            .OrderBy(rt => rt.TypeName)
            .Select(rt => new SelectListItem
            {
                Value = rt.ResourceTypeId.ToString(),
                Text = rt.TypeName
            })
            .ToListAsync();

        // Add "All Types" as first option
        types.Insert(0, new SelectListItem { Value = "", Text = "All Types" });

        return types;
    }

    public async Task<ResourceViewModel?> GetResourceByIdAsync(Guid resourceId)
    {
        return await _context.Resources
            .Include(r => r.ResourceType)
            .Where(r => r.ResourceId == resourceId && !r.IsDeleted && r.IsActive)
            .Select(r => new ResourceViewModel
            {
                ResourceId = r.ResourceId,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location,
                TypeName = r.ResourceType.TypeName,
                ResourceTypeId = r.ResourceTypeId,
                IsActive = r.IsActive
            })
            .FirstOrDefaultAsync();
    }
}