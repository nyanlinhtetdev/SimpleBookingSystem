namespace SimpleBookingSystem.ViewModels.Booking;

public class CreateBookingViewModel
{
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public string? Location { get; set; }

    // Filled by JavaScript — no [Required] attribute
    // Validation is done manually in the controller
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}