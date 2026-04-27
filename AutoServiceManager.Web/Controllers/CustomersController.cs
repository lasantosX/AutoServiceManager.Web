using AutoServiceManager.Web.Data;
using AutoServiceManager.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceManager.Web.Controllers;

public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        ViewData["CurrentFilter"] = searchTerm;

        var customersQuery = _context.Customers
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            customersQuery = customersQuery.Where(customer =>
                customer.FirstName.Contains(searchTerm) ||
                customer.LastName.Contains(searchTerm) ||
                (customer.Email != null && customer.Email.Contains(searchTerm)) ||
                (customer.PhoneNumber != null && customer.PhoneNumber.Contains(searchTerm)));
        }

        var customers = await customersQuery
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .ToListAsync();

        return View(customers);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var customer = await _context.Customers
            .AsNoTracking()
            .Include(customer => customer.Vehicles)
            .Include(customer => customer.ServiceOrders)
            .FirstOrDefaultAsync(customer => customer.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        return View(customer);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        customer.RowCreatedDate = DateTime.UtcNow;

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Customer created successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (id != customer.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        try
        {
            var existingCustomer = await _context.Customers.FindAsync(id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Email = customer.Email;
            existingCustomer.PhoneNumber = customer.PhoneNumber;
            existingCustomer.Address = customer.Address;
            existingCustomer.RowModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer updated successfully.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CustomerExists(customer.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        return View(customer);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await _context.Customers
            .Include(customer => customer.Vehicles)
            .Include(customer => customer.ServiceOrders)
            .FirstOrDefaultAsync(customer => customer.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        if (customer.Vehicles.Any() || customer.ServiceOrders.Any())
        {
            TempData["ErrorMessage"] = "Customer cannot be deleted because it has related vehicles or service orders.";
            return RedirectToAction(nameof(Index));
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Customer deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CustomerExists(int id)
    {
        return await _context.Customers.AnyAsync(customer => customer.Id == id);
    }
}