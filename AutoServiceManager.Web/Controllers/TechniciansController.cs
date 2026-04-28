using AutoServiceManager.Web.Data;
using AutoServiceManager.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceManager.Web.Controllers;

public class TechniciansController : Controller
{
    private readonly ApplicationDbContext _context;

    public TechniciansController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        ViewData["CurrentFilter"] = searchTerm;

        var techniciansQuery = _context.Technicians
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            techniciansQuery = techniciansQuery.Where(technician =>
                technician.FullName.Contains(searchTerm) ||
                technician.Specialty.Contains(searchTerm));
        }

        var technicians = await techniciansQuery
            .OrderByDescending(technician => technician.IsActive)
            .ThenBy(technician => technician.FullName)
            .ToListAsync();

        return View(technicians);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var technician = await _context.Technicians
            .AsNoTracking()
            .Include(technician => technician.ServiceOrders)
            .FirstOrDefaultAsync(technician => technician.Id == id);

        if (technician == null)
        {
            return NotFound();
        }

        return View(technician);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Technician technician)
    {
        if (!ModelState.IsValid)
        {
            return View(technician);
        }

        technician.RowCreatedDate = DateTime.UtcNow;

        _context.Technicians.Add(technician);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Technician created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var technician = await _context.Technicians.FindAsync(id);

        if (technician == null)
        {
            return NotFound();
        }

        return View(technician);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Technician technician)
    {
        if (id != technician.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(technician);
        }

        var existingTechnician = await _context.Technicians.FindAsync(id);

        if (existingTechnician == null)
        {
            return NotFound();
        }

        existingTechnician.FullName = technician.FullName;
        existingTechnician.Specialty = technician.Specialty;
        existingTechnician.IsActive = technician.IsActive;
        existingTechnician.RowModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Technician updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var technician = await _context.Technicians
            .AsNoTracking()
            .FirstOrDefaultAsync(technician => technician.Id == id);

        if (technician == null)
        {
            return NotFound();
        }

        return View(technician);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var technician = await _context.Technicians
            .Include(technician => technician.ServiceOrders)
            .FirstOrDefaultAsync(technician => technician.Id == id);

        if (technician == null)
        {
            return NotFound();
        }

        if (technician.ServiceOrders.Any())
        {
            TempData["ErrorMessage"] = "Technician cannot be deleted because it has related service orders.";
            return RedirectToAction(nameof(Index));
        }

        _context.Technicians.Remove(technician);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Technician deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}