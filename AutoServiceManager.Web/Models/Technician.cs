using System.ComponentModel.DataAnnotations;

namespace AutoServiceManager.Web.Models;

public class Technician
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Created Date")]
    public DateTime RowCreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Modified Date")]
    public DateTime? RowModifiedDate { get; set; }

    public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
}