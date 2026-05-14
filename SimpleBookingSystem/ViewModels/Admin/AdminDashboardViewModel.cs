namespace SimpleBookingSystem.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        // Summary counts for dashboard cards
        public int TotalUsers { get; set; }
        public int TotalResources { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CancelledBookings { get; set; }

        // Recent bookings shown on dashboard table
        public List<AdminBookingViewModel> RecentBookings { get; set; } = new();
    }
}
