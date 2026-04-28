using AutoServiceManager.Web.Data;
using AutoServiceManager.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceManager.Web.Controllers;

public class OperationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public OperationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Create(int? serviceOrderId)
    {
        if (serviceOrderId == null)
        {
            return NotFound();
        }

        var serviceOrder = await _context.ServiceOrders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Vehicle)
            .FirstOrDefaultAsync(order => order.Id == serviceOrderId);

        if (serviceOrder == null)
        {
            return NotFound();
        }

        ViewBag.ServiceOrder = serviceOrder;

        var operation = new Operation
        {
            ServiceOrderId = serviceOrder.Id,
            LaborRate = 120.00m
        };

        return View(operation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Operation operation)
    {
        var serviceOrder = await _context.ServiceOrders
            .Include(order => order.Operations)
            .FirstOrDefaultAsync(order => order.Id == operation.ServiceOrderId);

        if (serviceOrder == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ServiceOrder = await GetServiceOrderHeaderAsync(operation.ServiceOrderId);
            return View(operation);
        }

        operation.Recalculate();
        operation.RowCreatedDate = DateTime.UtcNow;

        _context.Operations.Add(operation);
        await _context.SaveChangesAsync();

        await RecalculateServiceOrderTotalsAsync(operation.ServiceOrderId);

        TempData["SuccessMessage"] = "Operation added successfully.";

        return RedirectToAction("Details", "ServiceOrders", new { id = operation.ServiceOrderId });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var operation = await _context.Operations
            .AsNoTracking()
            .FirstOrDefaultAsync(operation => operation.Id == id);

        if (operation == null)
        {
            return NotFound();
        }

        ViewBag.ServiceOrder = await GetServiceOrderHeaderAsync(operation.ServiceOrderId);

        return View(operation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Operation operation)
    {
        if (id != operation.Id)
        {
            return NotFound();
        }

        var existingOperation = await _context.Operations.FindAsync(id);

        if (existingOperation == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ServiceOrder = await GetServiceOrderHeaderAsync(operation.ServiceOrderId);
            return View(operation);
        }

        existingOperation.Description = operation.Description;
        existingOperation.LaborHours = operation.LaborHours;
        existingOperation.LaborRate = operation.LaborRate;
        existingOperation.PartsAmount = operation.PartsAmount;
        existingOperation.RowModifiedDate = DateTime.UtcNow;
        existingOperation.Recalculate();

        await _context.SaveChangesAsync();

        await RecalculateServiceOrderTotalsAsync(existingOperation.ServiceOrderId);

        TempData["SuccessMessage"] = "Operation updated successfully.";

        return RedirectToAction("Details", "ServiceOrders", new { id = existingOperation.ServiceOrderId });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var operation = await _context.Operations
            .AsNoTracking()
            .Include(operation => operation.ServiceOrder)
                .ThenInclude(order => order!.Customer)
            .Include(operation => operation.ServiceOrder)
                .ThenInclude(order => order!.Vehicle)
            .FirstOrDefaultAsync(operation => operation.Id == id);

        if (operation == null)
        {
            return NotFound();
        }

        return View(operation);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var operation = await _context.Operations.FindAsync(id);

        if (operation == null)
        {
            return NotFound();
        }

        var serviceOrderId = operation.ServiceOrderId;

        _context.Operations.Remove(operation);
        await _context.SaveChangesAsync();

        await RecalculateServiceOrderTotalsAsync(serviceOrderId);

        TempData["SuccessMessage"] = "Operation deleted successfully.";

        return RedirectToAction("Details", "ServiceOrders", new { id = serviceOrderId });
    }

    private async Task<ServiceOrder?> GetServiceOrderHeaderAsync(int serviceOrderId)
    {
        return await _context.ServiceOrders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Vehicle)
            .FirstOrDefaultAsync(order => order.Id == serviceOrderId);
    }

    private async Task RecalculateServiceOrderTotalsAsync(int serviceOrderId)
    {
        var serviceOrder = await _context.ServiceOrders
            .Include(order => order.Operations)
            .FirstOrDefaultAsync(order => order.Id == serviceOrderId);

        if (serviceOrder == null)
        {
            return;
        }

        serviceOrder.LaborTotal = serviceOrder.Operations.Sum(operation => operation.LaborAmount);
        serviceOrder.PartsTotal = serviceOrder.Operations.Sum(operation => operation.PartsAmount);
        serviceOrder.GrandTotal = serviceOrder.Operations.Sum(operation => operation.TotalAmount);
        serviceOrder.RowModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}