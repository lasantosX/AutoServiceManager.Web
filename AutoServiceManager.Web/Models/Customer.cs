using System.ComponentModel.DataAnnotations;

namespace AutoServiceManager.Web.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(30)]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [Display(Name = "Created Date")]
    public DateTime RowCreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Modified Date")]
    public DateTime? RowModifiedDate { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

    [Display(Name = "Customer")]
    public string FullName => $"{FirstName} {LastName}";
}