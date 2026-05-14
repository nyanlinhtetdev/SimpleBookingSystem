using Microsoft.AspNetCore.Mvc;
using SimpleBookingSystem.Services;
using SimpleBookingSystem.ViewModels.Resource;

namespace SimpleBookingSystem.Controllers;

public class ResourceController : Controller
{
    private readonly IResourceService _resourceService;

    public ResourceController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    // ── Index — keyword search (Option 1) ─────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword)
    {
        var model = new ResourceSearchViewModel
        {
            Keyword = keyword,
            Resources = await _resourceService.SearchResourcesAsync(keyword, null),
            ResourceTypes = await _resourceService.GetResourceTypeSelectListAsync()
        };

        return View(model);
    }

    // ── FilterByType — AJAX endpoint for type buttons (Option 2) ─────────────
    // Returns only the partial HTML of resource cards — no full page reload
    [HttpGet]
    public async Task<IActionResult> FilterByType(Guid? resourceTypeId)
    {
        var resources = await _resourceService.SearchResourcesAsync(null, resourceTypeId);
        return PartialView("_ResourceCards", resources);
    }
}