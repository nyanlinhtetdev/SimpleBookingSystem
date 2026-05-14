namespace SimpleBookingSystem.ViewModels.Booking;

public class EditBookingViewModel
{
    public Guid BookingId { get; set; }
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public string? Location { get; set; }

    // Filled by JavaScript — validated manually in controller
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}