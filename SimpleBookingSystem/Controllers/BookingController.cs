using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleBookingSystem.Services;
using SimpleBookingSystem.ViewModels.Booking;
using System.Security.Claims;

namespace SimpleBookingSystem.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IResourceService _resourceService;

    public BookingController(IBookingService bookingService, IResourceService resourceService)
    {
        _bookingService = bookingService;
        _resourceService = resourceService;
    }

    // Helper — gets logged-in user's ID from JWT claims
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── My Bookings ───────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        var bookings = await _bookingService.GetUserBookingsAsync(GetUserId());
        return View(bookings);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Create(Guid resourceId)
    {
        var resource = await _resourceService.GetResourceByIdAsync(resourceId);
        if (resource == null)
            return NotFound();

        var model = new CreateBookingViewModel
        {
            ResourceId = resource.ResourceId,
            ResourceName = resource.Name,
            TypeName = resource.TypeName,
            Location = resource.Location,
            // Default: tomorrow 9AM to 10AM
            StartTime = DateTime.Today.AddDays(1).AddHours(9),
            EndTime = DateTime.Today.AddDays(1).AddHours(10)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingViewModel model)
    {
        // Manually validate times since they come from JavaScript hidden fields
        if (model.StartTime == default || model.EndTime == default)
        {
            TempData["Error"] = "Please select a time slot from the calendar.";
            var resource = await _resourceService.GetResourceByIdAsync(model.ResourceId);
            if (resource != null)
            {
                model.ResourceName = resource.Name;
                model.TypeName = resource.TypeName;
                model.Location = resource.Location;
            }
            return View(model);
        }

        if (model.EndTime <= model.StartTime)
        {
            TempData["Error"] = "End time must be after start time.";
            return View(model);
        }

        var success = await _bookingService.CreateBookingAsync(GetUserId(), model);

        if (!success)
        {
            TempData["Error"] = "This time slot is already booked. Please choose a different time.";
            return View(model);
        }

        TempData["Success"] = "Booking created successfully!";
        return RedirectToAction("MyBookings");
    }

    // ── Edit ──────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(Guid bookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(bookingId, GetUserId());
        if (booking == null)
            return NotFound();

        // Only active bookings can be edited
        if (booking.Status != "Active")
        {
            TempData["Error"] = "Only active bookings can be edited.";
            return RedirectToAction("MyBookings");
        }

        var model = new EditBookingViewModel
        {
            BookingId = booking.BookingId,
            ResourceId = booking.ResourceId,
            ResourceName = booking.ResourceName,
            TypeName = booking.TypeName,
            Location = booking.Location,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditBookingViewModel model)
    {
        if (model.StartTime == default || model.EndTime == default)
        {
            TempData["Error"] = "Please select a new time slot from the calendar.";
            var booking = await _bookingService.GetBookingByIdAsync(model.BookingId, GetUserId());
            if (booking != null)
            {
                model.ResourceName = booking.ResourceName;
                model.TypeName = booking.TypeName;
                model.Location = booking.Location;
                model.StartTime = booking.StartTime;
                model.EndTime = booking.EndTime;
            }
            return View(model);
        }

        if (model.EndTime <= model.StartTime)
        {
            TempData["Error"] = "End time must be after start time.";
            return View(model);
        }

        var success = await _bookingService.EditBookingAsync(GetUserId(), model);

        if (!success)
        {
            TempData["Error"] = "This time slot is already booked. Please choose a different time.";
            return View(model);
        }

        TempData["Success"] = "Booking updated successfully!";
        return RedirectToAction("MyBookings");
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Cancel(Guid bookingId)
    {
        var success = await _bookingService.CancelBookingAsync(bookingId, GetUserId());

        TempData[success ? "Success" : "Error"] = success
            ? "Booking cancelled successfully."
            : "Booking could not be cancelled.";

        return RedirectToAction("MyBookings");
    }

    // ── AJAX — Get Booked Slots for Calendar ──────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetBookedSlots(Guid resourceId, DateTime startDate)
    {
        var slots = await _bookingService.GetBookedSlotsAsync(resourceId, startDate);
        return Json(slots);
    }
}