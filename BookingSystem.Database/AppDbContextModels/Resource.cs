using System;
using System.Collections.Generic;

namespace BookingSystem.Database.AppDbContextModels;

public partial class Resource
{
    public Guid ResourceId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid ResourceTypeId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ResourceType ResourceType { get; set; } = null!;
}
