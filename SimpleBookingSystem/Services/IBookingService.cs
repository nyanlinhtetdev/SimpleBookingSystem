using SimpleBookingSystem.ViewModels.Booking;

namespace SimpleBookingSystem.Services;

public interface IBookingService
{
    Task<bool> HasConflictAsync(Guid resourceId, DateTime start, DateTime end, Guid? excludeBookingId = null);

    Task<List<BookingViewModel>> GetUserBookingsAsync(Guid userId);

    Task<BookingViewModel?> GetBookingByIdAsync(Guid bookingId, Guid userId);

    Task<List<object>> GetBookedSlotsAsync(Guid resourceId, DateTime startDate, DateTime endDate);
    Task<bool> CreateBookingAsync(Guid userId, CreateBookingViewModel model);

    Task<bool> EditBookingAsync(Guid userId, EditBookingViewModel model);

    Task<bool> CancelBookingAsync(Guid bookingId, Guid userId);
}