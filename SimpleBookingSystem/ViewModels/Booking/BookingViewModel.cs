namespace SimpleBookingSystem.ViewModels.Booking
{
    public class BookingViewModel
    {
        public Guid BookingId { get; set; }
        public Guid ResourceId { get; set; }
        public string ResourceName { get; set; } = null!;
        public string TypeName { get; set; } = null!;
        public string? Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
