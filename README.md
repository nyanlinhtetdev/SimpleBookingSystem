# SimpleBookingSystem

The Simple Booking System is a web application built with ASP.NET Core 8 MVC. It serves as a platform for booking shared resources within an organisation. The system allows users to browse and book resources such as meeting rooms, lab equipment, and study pods, while administrators can manage all bookings, resources, and users.

## Tech Stack
- **Framework:** .NET 8 (ASP.NET Core MVC)
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core (Database First)
- **Authentication:** JWT with Refresh Token support
- **Frontend:** Razor Views, HTML, Bootstrap 5

## Key Features
- User Authentication (JWT + Refresh Token Rotation)
- Role-based Authorization (User / Admin)
- Resource Browsing with Search and Type Filter
- Availability Calendar with Selectable Time Slots
- Booking Management (Create, Edit, Cancel)
- Admin Dashboard with Booking and User Overview


## Project Structure
- **SimpleBookingSystem**: The core MVC project containing Controllers, Services, ViewModels, Views, and Middlewares.
- **BookingSystem.Database**: A Class Library project for the data access layer, housing the EF Core AppDbContext and database models generated via scaffold.

## Database Table Structures

### Users

```sql
CREATE TABLE Users (
    UserID       UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName     NVARCHAR(100)  NOT NULL,
    Email        NVARCHAR(100)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255)  NOT NULL,
    Role         NVARCHAR(20)   NOT NULL DEFAULT 'User',
    IsActive     BIT            NOT NULL DEFAULT 1,
    IsDeleted    BIT            NOT NULL DEFAULT 0,
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE()
);
```
### Resources

```sql
CREATE TABLE Resources (
    ResourceID     UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name           NVARCHAR(100)    NOT NULL,
    Description    NVARCHAR(255)    NULL,
    Location       NVARCHAR(100)    NULL,
    IsActive       BIT              NOT NULL DEFAULT 1,
    IsDeleted      BIT              NOT NULL DEFAULT 0,
    CreatedAt      DATETIME         NOT NULL DEFAULT GETDATE(),
    ResourceTypeID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES ResourceTypes(ResourceTypeID)
);
```

### Bookings

```sql
CREATE TABLE Bookings (
    BookingID  UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserID     UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    ResourceID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Resources(ResourceID),
    StartTime  DATETIME         NOT NULL,
    EndTime    DATETIME         NOT NULL,
    Status     NVARCHAR(20)     NOT NULL DEFAULT 'Active',
    CreatedAt  DATETIME         NOT NULL DEFAULT GETDATE()
);
```

### ResourceTypes

```sql
CREATE TABLE ResourceTypes (
    ResourceTypeID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TypeName       NVARCHAR(50)  NOT NULL UNIQUE,
    IsDeleted      BIT           NOT NULL DEFAULT 0,
    CreatedAt      DATETIME      NOT NULL DEFAULT GETDATE()
);
```

### RefreshTokens

```sql
CREATE TABLE RefreshTokens (
    RefreshTokenID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserID         UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    Token          NVARCHAR(255)    NOT NULL,
    ExpiryDate     DATETIME         NOT NULL,
    IsRevoked      BIT              NOT NULL DEFAULT 0,
    CreatedAt      DATETIME         NOT NULL DEFAULT GETDATE()
);
```
## Database First Approach

This project uses a **Database-First approach** with Entity Framework Core.

- The database schema is designed and created directly in SQL Server via SSMS
- EF Core models and AppDbContext are generated using the Scaffold-DbContext command
- After scaffolding, the hardcoded connection string is removed from AppDbContext.cs and managed solely through appsettings.json

### Scaffold Command

Open terminal of BookingSystem.Database class library project and run command below
```bash
dotnet ef dbcontext scaffold "Server=YOUR_SERVER;Database=BookingDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o AppDbContextModels -c AppDbContext -f
```
