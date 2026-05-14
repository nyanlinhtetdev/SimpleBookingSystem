using SimpleBookingSystem.ViewModels.Booking;

namespace SimpleBookingSystem.Services;

public interface IBookingService
{
    // Check if a time slot conflicts with existing active bookings
    Task<bool> HasConflictAsync(Guid resourceId, DateTime start, DateTime end, Guid? excludeBookingId = null);

    // Get all bookings belonging to a specific user
    Task<List<BookingViewModel>> GetUserBookingsAsync(Guid userId);

    // Get a single booking by ID (ensures it belongs to the user)
    Task<BookingViewModel?> GetBookingByIdAsync(Guid bookingId, Guid userId);

    // Get booked time slots for a resource for a week range (for calendar)
    Task<List<object>> GetBookedSlotsAsync(Guid resourceId, DateTime startDate);

    // Create a new booking
    Task<bool> CreateBookingAsync(Guid userId, CreateBookingViewModel model);

    // Edit an existing booking
    Task<bool> EditBookingAsync(Guid userId, EditBookingViewModel model);

    // Cancel a booking (soft — sets Status to Cancelled)
    Task<bool> CancelBookingAsync(Guid bookingId, Guid userId);
}