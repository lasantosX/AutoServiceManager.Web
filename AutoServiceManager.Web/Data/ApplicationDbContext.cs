using AutoServiceManager.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceManager.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<Operation> Operations => Set<Operation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .HasMany(customer => customer.Vehicles)
            .WithOne(vehicle => vehicle.Customer)
            .HasForeignKey(vehicle => vehicle.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasMany(customer => customer.ServiceOrders)
            .WithOne(serviceOrder => serviceOrder.Customer)
            .HasForeignKey(serviceOrder => serviceOrder.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehicle>()
            .HasMany(vehicle => vehicle.ServiceOrders)
            .WithOne(serviceOrder => serviceOrder.Vehicle)
            .HasForeignKey(serviceOrder => serviceOrder.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Technician>()
            .HasMany(technician => technician.ServiceOrders)
            .WithOne(serviceOrder => serviceOrder.Technician)
            .HasForeignKey(serviceOrder => serviceOrder.TechnicianId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ServiceOrder>()
            .HasMany(serviceOrder => serviceOrder.Operations)
            .WithOne(operation => operation.ServiceOrder)
            .HasForeignKey(operation => operation.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.Vin)
            .IsUnique();
    }
}