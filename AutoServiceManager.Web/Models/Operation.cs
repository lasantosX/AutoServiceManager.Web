using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServiceManager.Web.Models;

public class Operation
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Service Order")]
    public int ServiceOrderId { get; set; }

    [Required]
    [StringLength(300)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 999.9)]
    [Column(TypeName = "decimal(18,1)")]
    [Display(Name = "Labor Hours")]
    public decimal LaborHours { get; set; }

    [Range(0, 9999.99)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Labor Rate")]
    public decimal LaborRate { get; set; }

    [Range(0, 999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Parts Amount")]
    public decimal PartsAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Labor Amount")]
    public decimal LaborAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Total Amount")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "Created Date")]
    public DateTime RowCreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Modified Date")]
    public DateTime? RowModifiedDate { get; set; }

    public ServiceOrder? ServiceOrder { get; set; }

    public void Recalculate()
    {
        LaborAmount = LaborHours * LaborRate;
        TotalAmount = LaborAmount + PartsAmount;
    }
}