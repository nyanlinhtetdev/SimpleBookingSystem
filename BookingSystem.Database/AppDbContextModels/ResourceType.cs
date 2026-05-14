using System;
using System.Collections.Generic;

namespace BookingSystem.Database.AppDbContextModels;

public partial class ResourceType
{
    public Guid ResourceTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
