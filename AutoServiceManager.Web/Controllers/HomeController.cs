using AutoServiceManager.Web.Data;
using AutoServiceManager.Web.Models;
using AutoServiceManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AutoServiceManager.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = new DashboardViewModel
        {
            TotalCustomers = await _context.Customers.CountAsync(),
            TotalVehicles = await _context.Vehicles.CountAsync(),
            TotalTechnicians = await _context.Technicians.CountAsync(),
            OpenServiceOrders = await _context.ServiceOrders
                .CountAsync(order => order.Status != ServiceOrderStatus.Closed && order.Status != ServiceOrderStatus.Cancelled),
            ClosedServiceOrders = await _context.ServiceOrders
                .CountAsync(order => order.Status == ServiceOrderStatus.Closed),
            TotalRevenue = await _context.ServiceOrders.SumAsync(order => order.GrandTotal),
            RecentServiceOrders = await _context.ServiceOrders
                .AsNoTracking()
                .Include(order => order.Customer)
                .Include(order => order.Vehicle)
                .Include(order => order.Technician)
                .OrderByDescending(order => order.OpenedDate)
                .Take(5)
                .ToListAsync()
        };

        return View(dashboard);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}