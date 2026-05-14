using System;
using System.Collections.Generic;

namespace BookingSystem.Database.AppDbContextModels;

public partial class Booking
{
    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public Guid ResourceId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Resource Resource { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
