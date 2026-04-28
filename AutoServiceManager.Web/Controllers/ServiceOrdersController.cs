using AutoServiceManager.Web.Data;
using AutoServiceManager.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceManager.Web.Controllers;

public class ServiceOrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServiceOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        ViewData["CurrentFilter"] = searchTerm;

        var serviceOrdersQuery = _context.ServiceOrders
            .AsNoTracking()
            .Include(serviceOrder => serviceOrder.Customer)
            .Include(serviceOrder => serviceOrder.Vehicle)
            .Include(serviceOrder => serviceOrder.Technician)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            serviceOrdersQuery = serviceOrdersQuery.Where(serviceOrder =>
                serviceOrder.Id.ToString().Contains(searchTerm) ||
                serviceOrder.Status.ToString().Contains(searchTerm) ||
                (serviceOrder.Customer != null &&
                    (serviceOrder.Customer.FirstName.Contains(searchTerm) ||
                     serviceOrder.Customer.LastName.Contains(searchTerm))) ||
                (serviceOrder.Vehicle != null &&
                    (serviceOrder.Vehicle.Vin.Contains(searchTerm) ||
                     serviceOrder.Vehicle.Make.Contains(searchTerm) ||
                     serviceOrder.Vehicle.Model.Contains(searchTerm))) ||
                (serviceOrder.Technician != null &&
                    serviceOrder.Technician.FullName.Contains(searchTerm)));
        }

        var serviceOrders = await serviceOrdersQuery
            .OrderByDescending(serviceOrder => serviceOrder.OpenedDate)
            .ToListAsync();

        return View(serviceOrders);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var serviceOrder = await _context.ServiceOrders
            .AsNoTracking()
            .Include(serviceOrder => serviceOrder.Customer)
            .Include(serviceOrder => serviceOrder.Vehicle)
            .Include(serviceOrder => serviceOrder.Technician)
            .Include(serviceOrder => serviceOrder.Operations)
            .FirstOrDefaultAsync(serviceOrder => serviceOrder.Id == id);

        if (serviceOrder == null)
        {
            return NotFound();
        }

        serviceOrder.Operations = serviceOrder.Operations
            .OrderBy(operation => operation.Id)
            .ToList();

        return View(serviceOrder);
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropDownListsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceOrder serviceOrder)
    {
        if (!await VehicleBelongsToCustomerAsync(serviceOrder.VehicleId, serviceOrder.CustomerId))
        {
            ModelState.AddModelError(nameof(serviceOrder.VehicleId), "The selected vehicle does not belong to the selected customer.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDropDownListsAsync(serviceOrder.CustomerId, serviceOrder.VehicleId, serviceOrder.TechnicianId);
            return View(serviceOrder);
        }

        serviceOrder.OpenedDate = DateTime.UtcNow;
        serviceOrder.RowCreatedDate = DateTime.UtcNow;
        serviceOrder.Status = ServiceOrderStatus.Open;

        _context.ServiceOrders.Add(serviceOrder);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Service order created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var serviceOrder = await _context.ServiceOrders.FindAsync(id);

        if (serviceOrder == null)
        {
            return NotFound();
        }

        await LoadDropDownListsAsync(serviceOrder.CustomerId, serviceOrder.VehicleId, serviceOrder.TechnicianId);

        return View(serviceOrder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceOrder serviceOrder)
    {
        if (id != serviceOrder.Id)
        {
            return NotFound();
        }

        if (!await VehicleBelongsToCustomerAsync(serviceOrder.VehicleId, serviceOrder.CustomerId))
        {
            ModelState.AddModelError(nameof(serviceOrder.VehicleId), "The selected vehicle does not belong to the selected customer.");
        }

        if (!ModelState.IsValid)
        {
            await LoadDropDownListsAsync(serviceOrder.CustomerId, serviceOrder.VehicleId, serviceOrder.TechnicianId);
            return View(serviceOrder);
        }

        var existingServiceOrder = await _context.ServiceOrders
            .Include(order => order.Operations)
            .FirstOrDefaultAsync(order => order.Id == id);

        if (existingServiceOrder == null)
        {
            return NotFound();
        }

        existingServiceOrder.CustomerId = serviceOrder.CustomerId;
        existingServiceOrder.VehicleId = serviceOrder.VehicleId;
        existingServiceOrder.TechnicianId = serviceOrder.TechnicianId;
        existingServiceOrder.Status = serviceOrder.Status;
        existingServiceOrder.Complaint = serviceOrder.Complaint;
        existingServiceOrder.RowModifiedDate = DateTime.UtcNow;

        if (serviceOrder.Status == ServiceOrderStatus.Closed && existingServiceOrder.ClosedDate == null)
        {
            if (!existingServiceOrder.Operations.Any())
            {
                ModelState.AddModelError(string.Empty, "A service order cannot be closed without operations.");
                await LoadDropDownListsAsync(serviceOrder.CustomerId, serviceOrder.VehicleId, serviceOrder.TechnicianId);
                return View(serviceOrder);
            }

            existingServiceOrder.ClosedDate = DateTime.UtcNow;
        }

        if (serviceOrder.Status != ServiceOrderStatus.Closed)
        {
            existingServiceOrder.ClosedDate = null;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Service order updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var serviceOrder = await _context.ServiceOrders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Vehicle)
            .Include(order => order.Technician)
            .Include(order => order.Operations)
            .FirstOrDefaultAsync(order => order.Id == id);

        if (serviceOrder == null)
        {
            return NotFound();
        }

        return View(serviceOrder);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var serviceOrder = await _context.ServiceOrders
            .Include(order => order.Operations)
            .FirstOrDefaultAsync(order => order.Id == id);

        if (serviceOrder == null)
        {
            return NotFound();
        }

        _context.ServiceOrders.Remove(serviceOrder);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Service order deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropDownListsAsync(
        int? selectedCustomerId = null,
        int? selectedVehicleId = null,
        int? selectedTechnicianId = null)
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .Select(customer => new
            {
                customer.Id,
                Name = customer.FirstName + " " + customer.LastName
            })
            .ToListAsync();

        var vehiclesQuery = _context.Vehicles
            .AsNoTracking()
            .AsQueryable();

        if (selectedCustomerId.HasValue && selectedCustomerId.Value > 0)
        {
            vehiclesQuery = vehiclesQuery.Where(vehicle => vehicle.CustomerId == selectedCustomerId.Value);
        }

        var vehicles = await vehiclesQuery
            .OrderBy(vehicle => vehicle.Make)
            .ThenBy(vehicle => vehicle.Model)
            .Select(vehicle => new
            {
                vehicle.Id,
                Name = vehicle.Year + " " + vehicle.Make + " " + vehicle.Model + " - " + vehicle.Vin
            })
            .ToListAsync();

        var technicians = await _context.Technicians
            .AsNoTracking()
            .Where(technician => technician.IsActive || technician.Id == selectedTechnicianId)
            .OrderBy(technician => technician.FullName)
            .Select(technician => new
            {
                technician.Id,
                technician.FullName
            })
            .ToListAsync();

        ViewBag.CustomerId = new SelectList(customers, "Id", "Name", selectedCustomerId);
        ViewBag.VehicleId = new SelectList(vehicles, "Id", "Name", selectedVehicleId);
        ViewBag.TechnicianId = new SelectList(technicians, "Id", "FullName", selectedTechnicianId);
        ViewBag.StatusList = new SelectList(Enum.GetValues<ServiceOrderStatus>(), selectedValue: null);
    }

    private async Task<bool> VehicleBelongsToCustomerAsync(int vehicleId, int customerId)
    {
        return await _context.Vehicles.AnyAsync(vehicle =>
            vehicle.Id == vehicleId &&
            vehicle.CustomerId == customerId);
    }
}