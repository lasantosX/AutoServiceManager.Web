using AutoServiceManager.Web.Data;
using AutoServiceManager.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceManager.Web.Controllers;

public class VehiclesController : Controller
{
    private readonly ApplicationDbContext _context;

    public VehiclesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        ViewData["CurrentFilter"] = searchTerm;

        var vehiclesQuery = _context.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            vehiclesQuery = vehiclesQuery.Where(vehicle =>
                vehicle.Vin.Contains(searchTerm) ||
                vehicle.Make.Contains(searchTerm) ||
                vehicle.Model.Contains(searchTerm) ||
                (vehicle.LicensePlate != null && vehicle.LicensePlate.Contains(searchTerm)) ||
                (vehicle.Customer != null &&
                    (vehicle.Customer.FirstName.Contains(searchTerm) ||
                     vehicle.Customer.LastName.Contains(searchTerm))));
        }

        var vehicles = await vehiclesQuery
            .OrderBy(vehicle => vehicle.Make)
            .ThenBy(vehicle => vehicle.Model)
            .ToListAsync();

        return View(vehicles);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.Customer)
            .Include(vehicle => vehicle.ServiceOrders)
            .FirstOrDefaultAsync(vehicle => vehicle.Id == id);

        if (vehicle == null)
        {
            return NotFound();
        }

        return View(vehicle);
    }

    public async Task<IActionResult> Create()
    {
        await LoadCustomersDropDownListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Vehicle vehicle)
    {
        if (await VinExistsAsync(vehicle.Vin))
        {
            ModelState.AddModelError(nameof(vehicle.Vin), "A vehicle with this VIN already exists.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCustomersDropDownListAsync(vehicle.CustomerId);
            return View(vehicle);
        }

        vehicle.RowCreatedDate = DateTime.UtcNow;

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Vehicle created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
        {
            return NotFound();
        }

        await LoadCustomersDropDownListAsync(vehicle.CustomerId);

        return View(vehicle);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Vehicle vehicle)
    {
        if (id != vehicle.Id)
        {
            return NotFound();
        }

        if (await VinExistsAsync(vehicle.Vin, vehicle.Id))
        {
            ModelState.AddModelError(nameof(vehicle.Vin), "A vehicle with this VIN already exists.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCustomersDropDownListAsync(vehicle.CustomerId);
            return View(vehicle);
        }

        var existingVehicle = await _context.Vehicles.FindAsync(id);

        if (existingVehicle == null)
        {
            return NotFound();
        }

        existingVehicle.CustomerId = vehicle.CustomerId;
        existingVehicle.Vin = vehicle.Vin;
        existingVehicle.Make = vehicle.Make;
        existingVehicle.Model = vehicle.Model;
        existingVehicle.Year = vehicle.Year;
        existingVehicle.LicensePlate = vehicle.LicensePlate;
        existingVehicle.RowModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Vehicle updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(vehicle => vehicle.Customer)
            .FirstOrDefaultAsync(vehicle => vehicle.Id == id);

        if (vehicle == null)
        {
            return NotFound();
        }

        return View(vehicle);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var vehicle = await _context.Vehicles
            .Include(vehicle => vehicle.ServiceOrders)
            .FirstOrDefaultAsync(vehicle => vehicle.Id == id);

        if (vehicle == null)
        {
            return NotFound();
        }

        if (vehicle.ServiceOrders.Any())
        {
            TempData["ErrorMessage"] = "Vehicle cannot be deleted because it has related service orders.";
            return RedirectToAction(nameof(Index));
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Vehicle deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCustomersDropDownListAsync(int? selectedCustomerId = null)
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

        ViewBag.CustomerId = new SelectList(customers, "Id", "Name", selectedCustomerId);
    }

    private async Task<bool> VinExistsAsync(string vin, int? vehicleId = null)
    {
        return await _context.Vehicles.AnyAsync(vehicle =>
            vehicle.Vin == vin &&
            (!vehicleId.HasValue || vehicle.Id != vehicleId.Value));
    }
}