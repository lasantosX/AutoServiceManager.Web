using AutoServiceManager.Web.Models;

namespace AutoServiceManager.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalCustomers { get; set; }
    public int TotalVehicles { get; set; }
    public int TotalTechnicians { get; set; }
    public int OpenServiceOrders { get; set; }
    public int ClosedServiceOrders { get; set; }
    public decimal TotalRevenue { get; set; }

    public List<ServiceOrder> RecentServiceOrders { get; set; } = new();
}