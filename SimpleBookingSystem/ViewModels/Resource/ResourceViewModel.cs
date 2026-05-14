namespace SimpleBookingSystem.ViewModels.Resource
{
    public class ResourceViewModel
    {
        public Guid ResourceId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string TypeName { get; set; } = null!;
        public Guid ResourceTypeId { get; set; }
        public bool IsActive { get; set; }
    }
}
