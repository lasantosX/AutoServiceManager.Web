using Azure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServiceManager.Web.Models;

public class ServiceOrder
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required]
    [Display(Name = "Vehicle")]
    public int VehicleId { get; set; }

    [Display(Name = "Technician")]
    public int? TechnicianId { get; set; }

    public ServiceOrderStatus Status { get; set; } = ServiceOrderStatus.Open;

    [StringLength(500)]
    public string? Complaint { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Labor Total")]
    public decimal LaborTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Parts Total")]
    public decimal PartsTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Grand Total")]
    public decimal GrandTotal { get; set; }

    [Display(Name = "Opened Date")]
    public DateTime OpenedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Closed Date")]
    public DateTime? ClosedDate { get; set; }

    [Display(Name = "Created Date")]
    public DateTime RowCreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Modified Date")]
    public DateTime? RowModifiedDate { get; set; }

    public Customer? Customer { get; set; }

    public Vehicle? Vehicle { get; set; }

    public Technician? Technician { get; set; }

    public ICollection<Operation> Operations { get; set; } = new List<Operation>();
}