using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SimpleBookingSystem.ViewModels.Admin;

public class AdminResourceViewModel
{
    public Guid ResourceId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string TypeName { get; set; } = null!;
    public Guid ResourceTypeId { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int TotalBookings { get; set; }
}

// Used for Create and Edit resource forms
public class ResourceFormViewModel
{
    public Guid? ResourceId { get; set; }  // null = create, has value = edit

    [Required(ErrorMessage = "Resource name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;

    [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Please select a resource type.")]
    public Guid ResourceTypeId { get; set; }

    public bool IsActive { get; set; } = true;

    // Dropdown options
    public List<SelectListItem> ResourceTypes { get; set; } = new();
}