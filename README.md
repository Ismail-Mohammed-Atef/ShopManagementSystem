# Shop Management System

## Overview
This is a powerful .NET Core MVC-based shop management system designed for seamless order management, financial tracking, and product management. It includes advanced filtering, purchasing features, and a secure authentication and authorization system. The system is built with SOLID principles and incorporates design patterns for maintainability and scalability.

## Features
- **Order Management**: Track, update, and manage customer orders efficiently.
- **Financial Dashboard**: View real-time data on revenue, expenses, and profits.
- **Product Filtering**: Advanced filtering by name, price, stock availability, and category.
- **Purchasing System**: Manage new stock purchases and update inventory.
- **Authentication & Authorization**: Secure login and role-based access control (RBAC).
- **AJAX Integration**: Smooth, dynamic interactions for a better user experience.
- **Design Patterns**: Uses Repository, Unit of Work, and Dependency Injection.
- **SOLID Principles**: Ensures clean architecture and maintainability.

## Installation
1. Clone the repository.
2. Open the project in Visual Studio.
3. Restore NuGet packages.
4. Update the database connection string in `appsettings.json`.
5. Apply database migrations:
   ```sh
   dotnet ef database update
   ```
6. Run the application:
   ```sh
   dotnet run
   ```

## Usage
1. **Admin Panel**: Manage orders, products, and users.
2. **Customer Dashboard**: Place and track orders.
3. **Filtering**: Use advanced search to find products.
4. **Financial Insights**: View revenue and expenses in the dashboard.

## Requirements
- **.NET Version**: .NET 8 or later
- **Database**: SQL Server
- **Frontend**: Razor Views with Bootstrap
- **Authentication**: ASP.NET Core Identity


## License
This project is licensed under the MIT License.

## Contact
For support or inquiries, reach out via email at ismail.mohammed.atef@gmail.com.


