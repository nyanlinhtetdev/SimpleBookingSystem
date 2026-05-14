using BookingSystem.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;
using SimpleBookingSystem.ViewModels.Booking;

namespace SimpleBookingSystem.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasConflictAsync(Guid resourceId, DateTime start, DateTime end, Guid? excludeBookingId = null)
    {
        // Check if any active booking overlaps with the requested time slot
        return await _context.Bookings.AnyAsync(b =>
            b.ResourceId == resourceId &&
            b.Status == "Active" &&
            b.BookingId != (excludeBookingId ?? Guid.Empty) &&
            b.StartTime < end &&
            b.EndTime > start);
    }

    public async Task<List<BookingViewModel>> GetUserBookingsAsync(Guid userId)
    {
        return await _context.Bookings
            .Include(b => b.Resource)
                .ThenInclude(r => r.ResourceType)
            .Where(b => b.UserId == userId && b.Status == "Active")
            .OrderByDescending(b => b.StartTime)
            .Select(b => new BookingViewModel
            {
                BookingId = b.BookingId,
                ResourceId = b.ResourceId,
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

    public async Task<BookingViewModel?> GetBookingByIdAsync(Guid bookingId, Guid userId)
    {
        return await _context.Bookings
            .Include(b => b.Resource)
                .ThenInclude(r => r.ResourceType)
            .Where(b => b.BookingId == bookingId && b.UserId == userId)
            .Select(b => new BookingViewModel
            {
                BookingId = b.BookingId,
                ResourceId = b.ResourceId,
                ResourceName = b.Resource.Name,
                TypeName = b.Resource.ResourceType.TypeName,
                Location = b.Resource.Location,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<object>> GetBookedSlotsAsync(Guid resourceId, DateTime startDate)
    {
        // Fetch booked slots for the entire week view (7 days from startDate)
        // FullCalendar passes the start of the visible range, not just one day
        var endDate = startDate.AddDays(7);

        var slots = await _context.Bookings
            .Where(b => b.ResourceId == resourceId &&
                        b.Status == "Active" &&
                        b.StartTime >= startDate &&
                        b.StartTime < endDate)
            .Select(b => new
            {
                start = b.StartTime,
                end = b.EndTime
            })
            .ToListAsync();

        return slots.Cast<object>().ToList();
    }

    public async Task<bool> CreateBookingAsync(Guid userId, CreateBookingViewModel model)
    {
        // Validate: end time must be after start time
        if (model.EndTime <= model.StartTime)
            return false;

        // Validate: no conflicting bookings
        if (await HasConflictAsync(model.ResourceId, model.StartTime, model.EndTime))
            return false;

        var booking = new Booking
        {
            UserId = userId,
            ResourceId = model.ResourceId,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EditBookingAsync(Guid userId, EditBookingViewModel model)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == model.BookingId &&
                                      b.UserId == userId &&
                                      b.Status == "Active");

        if (booking == null)
            return false;

        // Validate: end time must be after start time
        if (model.EndTime <= model.StartTime)
            return false;

        // Validate: no conflict — exclude current booking from conflict check
        if (await HasConflictAsync(model.ResourceId, model.StartTime, model.EndTime, model.BookingId))
            return false;

        booking.StartTime = model.StartTime;
        booking.EndTime = model.EndTime;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelBookingAsync(Guid bookingId, Guid userId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == bookingId &&
                                      b.UserId == userId &&
                                      b.Status == "Active");

        if (booking == null)
            return false;

        // Soft cancel — keep the record, just mark it cancelled
        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return true;
    }
}