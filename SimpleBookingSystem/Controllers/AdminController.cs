using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleBookingSystem.Services;
using SimpleBookingSystem.ViewModels.Admin;

namespace SimpleBookingSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IResourceService _resourceService;

    public AdminController(IAdminService adminService, IResourceService resourceService)
    {
        _adminService = adminService;
        _resourceService = resourceService;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var model = await _adminService.GetDashboardDataAsync();
        return View(model);
    }

    // ── All Bookings ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> AllBookings()
    {
        var bookings = await _adminService.GetAllBookingsAsync();
        return View(bookings);
    }

    [HttpPost]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        var success = await _adminService.CancelAnyBookingAsync(bookingId);

        TempData[success ? "Success" : "Error"] = success
            ? "Booking cancelled successfully."
            : "Booking could not be cancelled.";

        return RedirectToAction("AllBookings");
    }

    // ── Manage Users ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> ManageUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        await _adminService.DeactivateUserAsync(userId);
        TempData["Success"] = "User deactivated successfully.";
        return RedirectToAction("ManageUsers");
    }

    [HttpPost]
    public async Task<IActionResult> ReactivateUser(Guid userId)
    {
        await _adminService.ReactivateUserAsync(userId);
        TempData["Success"] = "User reactivated successfully.";
        return RedirectToAction("ManageUsers");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await _adminService.SoftDeleteUserAsync(userId);
        TempData["Success"] = "User removed successfully.";
        return RedirectToAction("ManageUsers");
    }

    // ── Manage Resources ──────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> ManageResources()
    {
        var resources = await _adminService.GetAllResourcesAsync();
        return View(resources);
    }

    [HttpGet]
    public async Task<IActionResult> CreateResource()
    {
        var model = new ResourceFormViewModel
        {
            ResourceTypes = await _resourceService.GetResourceTypeSelectListAsync()
        };

        // Remove "All Types" placeholder option — not valid for a form
        model.ResourceTypes.RemoveAt(0);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateResource(ResourceFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ResourceTypes = await _resourceService.GetResourceTypeSelectListAsync();
            model.ResourceTypes.RemoveAt(0);
            return View(model);
        }

        await _adminService.CreateResourceAsync(model);
        TempData["Success"] = "Resource created successfully.";
        return RedirectToAction("ManageResources");
    }

    [HttpGet]
    public async Task<IActionResult> EditResource(Guid resourceId)
    {
        var model = await _adminService.GetResourceFormAsync(resourceId);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditResource(ResourceFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ResourceTypes = await _resourceService.GetResourceTypeSelectListAsync();
            model.ResourceTypes.RemoveAt(0);
            return View(model);
        }

        var success = await _adminService.EditResourceAsync(model);
        TempData[success ? "Success" : "Error"] = success
            ? "Resource updated successfully."
            : "Resource could not be updated.";

        return RedirectToAction("ManageResources");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleResource(Guid resourceId)
    {
        await _adminService.ToggleResourceActiveAsync(resourceId);
        TempData["Success"] = "Resource status updated.";
        return RedirectToAction("ManageResources");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteResource(Guid resourceId)
    {
        await _adminService.SoftDeleteResourceAsync(resourceId);
        TempData["Success"] = "Resource removed successfully.";
        return RedirectToAction("ManageResources");
    }
}