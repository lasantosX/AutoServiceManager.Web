# AutoService Manager Web

AutoService Manager Web is an ASP.NET Core MVC application designed to manage automotive service workflows, including customers, vehicles, technicians, service orders, and repair operations.

This project was built as a portfolio application to demonstrate practical experience with ASP.NET Core MVC, Entity Framework Core, SQL Server, Razor Views, Bootstrap, relational data modeling, CRUD operations, and business rule implementation.

---

## Features

- Dashboard with business metrics
- Customer management
- Vehicle management
- Technician management
- Service order management
- Repair operation management
- Automatic service order total recalculation
- Search/filter support
- Entity relationships using Entity Framework Core
- SQL Server LocalDB support
- Seed data for demo/testing
- Validation using Data Annotations
- Bootstrap-based responsive UI

---

## Tech Stack

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server LocalDB
- Razor Views
- Bootstrap 5
- LINQ
- Visual Studio 2022
- Git / GitHub

---

## Main Modules

### Customers

Manage customer records, including:

- First name
- Last name
- Email
- Phone number
- Address

### Vehicles

Manage vehicles assigned to customers, including:

- VIN
- Make
- Model
- Year
- License plate
- Customer relationship

### Technicians

Manage service technicians, including:

- Full name
- Specialty
- Active/inactive status

### Service Orders

Manage repair/service orders, including:

- Customer
- Vehicle
- Technician
- Status
- Customer complaint
- Opened date
- Closed date
- Labor total
- Parts total
- Grand total

### Operations

Manage repair operations inside a service order, including:

- Description
- Labor hours
- Labor rate
- Parts amount
- Labor amount
- Total amount

Totals are automatically recalculated when operations are created, updated, or deleted.

---

## Business Rules

- A vehicle must belong to the selected customer when creating or editing a service order.
- A service order cannot be closed if it has no operations.
- Inactive technicians are not shown for new assignments.
- Customers with related vehicles or service orders cannot be deleted.
- Vehicles with related service orders cannot be deleted.
- Technicians with related service orders cannot be deleted.
- Service order totals are recalculated automatically based on operations.

---

## Database

The application uses SQL Server LocalDB by default.

Connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AutoServiceManagerMvcDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
Entity Relationships
Customer has many Vehicles
Customer has many Service Orders
Vehicle has many Service Orders
Technician has many Service Orders
Service Order has many Operations
How to Run
1. Clone the repository
git clone https://github.com/lasantosX/AutoServiceManager.Web.git
2. Open the project

Open the solution in Visual Studio 2022.

3. Restore NuGet packages
dotnet restore
4. Apply database migrations
dotnet ef database update
5. Run the application
dotnet run

Or run it directly from Visual Studio using IIS Express or HTTPS profile.

Entity Framework Commands

Create a migration:

dotnet ef migrations add InitialCreate

Update database:

dotnet ef database update

Remove last migration if needed:

dotnet ef migrations remove
Demo Data

The application includes seed data for:

Customers
Vehicles
Technicians
Service Orders
Operations

Seed data is automatically inserted when the database is empty.

Project Structure
AutoServiceManager.Web
│
├── Controllers
│   ├── CustomersController.cs
│   ├── VehiclesController.cs
│   ├── TechniciansController.cs
│   ├── ServiceOrdersController.cs
│   └── OperationsController.cs
│
├── Data
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
│
├── Models
│   ├── Customer.cs
│   ├── Vehicle.cs
│   ├── Technician.cs
│   ├── ServiceOrder.cs
│   ├── ServiceOrderStatus.cs
│   └── Operation.cs
│
├── ViewModels
│   └── DashboardViewModel.cs
│
├── Views
│   ├── Customers
│   ├── Vehicles
│   ├── Technicians
│   ├── ServiceOrders
│   ├── Operations
│   ├── Home
│   └── Shared
│
├── wwwroot
├── Program.cs
└── appsettings.json
Purpose of the Project

This project demonstrates the ability to build a real-world ASP.NET Core MVC application with:

Clean controller logic
Entity Framework Core data access
Relational database modeling
Business validation
Razor Views
Bootstrap UI
CRUD workflows
Automatic calculations
Practical automotive service domain logic
Future Improvements

Possible next enhancements:

User authentication and roles
Pagination
Advanced filtering
Export service orders to PDF
Unit tests
Repository/service layer abstraction
Azure deployment
Docker support
API integration with a separate ASP.NET Core Web API
Author

Luis Angel Santos Guevara
Senior Software Developer
.NET | ASP.NET Core | SQL Server | MVC | Web API | Azure | Full Stack Development