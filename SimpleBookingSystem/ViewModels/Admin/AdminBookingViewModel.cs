namespace SimpleBookingSystem.ViewModels.Admin
{
    public class AdminBookingViewModel
    {
        public Guid BookingId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string ResourceName { get; set; } = null!;
        public string TypeName { get; set; } = null!;
        public string? Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
