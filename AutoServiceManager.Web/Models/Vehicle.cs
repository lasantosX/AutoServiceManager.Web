using System.ComponentModel.DataAnnotations;

namespace AutoServiceManager.Web.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(17)]
    [Display(Name = "VIN")]
    public string Vin { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Year { get; set; }

    [StringLength(30)]
    [Display(Name = "License Plate")]
    public string? LicensePlate { get; set; }

    [Display(Name = "Created Date")]
    public DateTime RowCreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Modified Date")]
    public DateTime? RowModifiedDate { get; set; }

    public Customer? Customer { get; set; }

    public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

    [Display(Name = "Vehicle")]
    public string DisplayName => $"{Year} {Make} {Model}";
}