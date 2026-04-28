using AutoServiceManager.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceManager.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Customers.AnyAsync())
        {
            return;
        }

        var customers = new List<Customer>
        {
            new()
            {
                FirstName = "Michael",
                LastName = "Johnson",
                Email = "michael.johnson@example.com",
                PhoneNumber = "555-1001",
                Address = "120 Main Street"
            },
            new()
            {
                FirstName = "Sarah",
                LastName = "Williams",
                Email = "sarah.williams@example.com",
                PhoneNumber = "555-1002",
                Address = "455 Oak Avenue"
            }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        var vehicles = new List<Vehicle>
        {
            new()
            {
                CustomerId = customers[0].Id,
                Vin = "1HGCM82633A004352",
                Year = 2021,
                Make = "Honda",
                Model = "Accord",
                LicensePlate = "ASM-1001"
            },
            new()
            {
                CustomerId = customers[1].Id,
                Vin = "2T1BURHE5JC034567",
                Year = 2020,
                Make = "Toyota",
                Model = "Corolla",
                LicensePlate = "ASM-1002"
            }
        };

        context.Vehicles.AddRange(vehicles);

        var technicians = new List<Technician>
        {
            new()
            {
                FullName = "David Miller",
                Specialty = "Engine Diagnostics",
                IsActive = true
            },
            new()
            {
                FullName = "Robert Garcia",
                Specialty = "Brake Systems",
                IsActive = true
            }
        };

        context.Technicians.AddRange(technicians);
        await context.SaveChangesAsync();

        var serviceOrders = new List<ServiceOrder>
        {
            new()
            {
                CustomerId = customers[0].Id,
                VehicleId = vehicles[0].Id,
                TechnicianId = technicians[0].Id,
                Status = ServiceOrderStatus.InProgress,
                Complaint = "Customer reports engine noise during acceleration.",
                OpenedDate = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                CustomerId = customers[1].Id,
                VehicleId = vehicles[1].Id,
                TechnicianId = technicians[1].Id,
                Status = ServiceOrderStatus.Completed,
                Complaint = "Brake pedal vibration when stopping.",
                OpenedDate = DateTime.UtcNow.AddDays(-5)
            }
        };

        context.ServiceOrders.AddRange(serviceOrders);
        await context.SaveChangesAsync();

        var operations = new List<Operation>
        {
            new()
            {
                ServiceOrderId = serviceOrders[0].Id,
                Description = "Perform engine diagnostic inspection",
                LaborHours = 1.5m,
                LaborRate = 120.00m,
                PartsAmount = 0.00m
            },
            new()
            {
                ServiceOrderId = serviceOrders[1].Id,
                Description = "Replace front brake pads",
                LaborHours = 2.0m,
                LaborRate = 120.00m,
                PartsAmount = 185.50m
            }
        };

        foreach (var operation in operations)
        {
            operation.Recalculate();
        }

        context.Operations.AddRange(operations);
        await context.SaveChangesAsync();

        foreach (var serviceOrder in serviceOrders)
        {
            var orderOperations = await context.Operations
                .Where(operation => operation.ServiceOrderId == serviceOrder.Id)
                .ToListAsync();

            serviceOrder.LaborTotal = orderOperations.Sum(operation => operation.LaborAmount);
            serviceOrder.PartsTotal = orderOperations.Sum(operation => operation.PartsAmount);
            serviceOrder.GrandTotal = orderOperations.Sum(operation => operation.TotalAmount);
            serviceOrder.RowModifiedDate = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }
}