namespace SimpleBookingSystem.ViewModels.Admin
{
    public class AdminUserViewModel
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }

        // Total bookings made by this user
        public int TotalBookings { get; set; }
    }
}
