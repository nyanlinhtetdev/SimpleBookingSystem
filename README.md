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

### Create admin account manually
```sql
INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive, IsDeleted)
VALUES (
    'Super Admin',
    'admin@bookingsystem.com',
    '$2a$11$dy0RcYZ/Qk4RXqgIJqDOhu/eGDYtdv2jVkrc/a26iHwEj.0s.cXge', -- raw password: admin123
    'Admin',
    1,
    0
);
```

### Insert Resource Data
```sql
INSERT INTO ResourceTypes (TypeName) VALUES
('Meeting Room'),
('Study Pod'),
('Lab Equipment'),
('Presentation Room'),
('Training Room'),
('Recording Studio'),
('Sports Facility'),
('Event Hall');

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Meeting Room Alpha', ResourceTypeID, 'Boardroom with projector and whiteboard, seats 10', 'Block A, Level 2', 1, 0
FROM ResourceTypes WHERE TypeName = 'Meeting Room';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Meeting Room Beta', ResourceTypeID, 'Medium-sized room with TV screen, seats 6', 'Block A, Level 3', 1, 0
FROM ResourceTypes WHERE TypeName = 'Meeting Room';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Meeting Room Gamma', ResourceTypeID, 'Small huddle room for quick discussions, seats 4', 'Block B, Level 1', 1, 0
FROM ResourceTypes WHERE TypeName = 'Meeting Room';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Meeting Room Delta', ResourceTypeID, 'Executive boardroom with video conferencing, seats 12', 'Block B, Level 4', 1, 0
FROM ResourceTypes WHERE TypeName = 'Meeting Room';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Meeting Room Epsilon', ResourceTypeID, 'Open-concept meeting space with movable chairs, seats 8', 'Block C, Level 2', 1, 0
FROM ResourceTypes WHERE TypeName = 'Meeting Room';

-- Study Pods (4)
INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Study Pod 1', ResourceTypeID, 'Quiet individual study pod with power outlets', 'Library, Level 1', 1, 0
FROM ResourceTypes WHERE TypeName = 'Study Pod';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Study Pod 2', ResourceTypeID, 'Soundproof pod ideal for video calls or focused work', 'Library, Level 1', 1, 0
FROM ResourceTypes WHERE TypeName = 'Study Pod';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Study Pod 3', ResourceTypeID, 'Collaborative pod for small groups, seats 3', 'Library, Level 2', 1, 0
FROM ResourceTypes WHERE TypeName = 'Study Pod';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Study Pod 4', ResourceTypeID, 'Standing desk pod with adjustable height', 'Library, Level 2', 1, 0
FROM ResourceTypes WHERE TypeName = 'Study Pod';

-- Lab Equipment (5)
INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT '3D Printer - Creality K1', ResourceTypeID, 'High-speed 3D printer for prototyping', 'Lab Room, Block D', 1, 0
FROM ResourceTypes WHERE TypeName = 'Lab Equipment';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Oscilloscope - Rigol DS1054', ResourceTypeID, '4-channel digital oscilloscope for circuit testing', 'Lab Room, Block D', 1, 0
FROM ResourceTypes WHERE TypeName = 'Lab Equipment';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Soldering Station', ResourceTypeID, 'Professional soldering station with temperature control', 'Lab Room, Block D', 1, 0
FROM ResourceTypes WHERE TypeName = 'Lab Equipment';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Raspberry Pi Kit', ResourceTypeID, 'Raspberry Pi 4 with accessories for IoT projects', 'Lab Room, Block D', 1, 0
FROM ResourceTypes WHERE TypeName = 'Lab Equipment';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Arduino Starter Kit', ResourceTypeID, 'Arduino Uno kit with sensors and components', 'Lab Room, Block D', 1, 0
FROM ResourceTypes WHERE TypeName = 'Lab Equipment';

-- Presentation Rooms (3)
INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Presentation Room 1', ResourceTypeID, 'Large room with projector and stage, seats 30', 'Auditorium, Block E', 1, 0
FROM ResourceTypes WHERE TypeName = 'Presentation Room';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Presentation Room 2', ResourceTypeID, 'Medium room with dual screens, seats 20', 'Auditorium, Block E', 1, 0
FROM ResourceTypes WHERE TypeName = 'Presentation Room';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Presentation Room 3', ResourceTypeID, 'Compact room for small presentations, seats 10', 'Block A, Level 1', 1, 0
FROM ResourceTypes WHERE TypeName = 'Presentation Room';

-- Training Rooms (3)
INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Training Room A', ResourceTypeID, 'Fully equipped training room with individual PCs, seats 20', 'Block F, Level 1', 1, 0
FROM ResourceTypes WHERE TypeName = 'Training Room';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Training Room B', ResourceTypeID, 'Training room with whiteboards and flip charts, seats 15', 'Block F, Level 2', 1, 0
FROM ResourceTypes WHERE TypeName = 'Training Room';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Training Room C', ResourceTypeID, 'Hands-on workshop room with workbenches, seats 12', 'Block F, Level 3', 1, 0
FROM ResourceTypes WHERE TypeName = 'Training Room';

-- Recording Studios (2)
INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Recording Studio A', ResourceTypeID, 'Professional podcast and video recording studio', 'Media Centre, Block G', 1, 0
FROM ResourceTypes WHERE TypeName = 'Recording Studio';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Recording Studio B', ResourceTypeID, 'Green screen studio for video production', 'Media Centre, Block G', 1, 0
FROM ResourceTypes WHERE TypeName = 'Recording Studio';

-- Sports Facilities (3)
INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Badminton Court 1', ResourceTypeID, 'Full-size badminton court with lighting', 'Sports Complex', 1, 0
FROM ResourceTypes WHERE TypeName = 'Sports Facility';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Badminton Court 2', ResourceTypeID, 'Full-size badminton court with lighting', 'Sports Complex', 1, 0
FROM ResourceTypes WHERE TypeName = 'Sports Facility';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Table Tennis Room', ResourceTypeID, 'Room with 2 table tennis tables', 'Sports Complex', 1, 0
FROM ResourceTypes WHERE TypeName = 'Sports Facility';

-- Event Halls (2)
INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Main Event Hall', ResourceTypeID, 'Large multipurpose hall for events and seminars, seats 100', 'Block H, Ground Floor', 1, 0
FROM ResourceTypes WHERE TypeName = 'Event Hall';

INSERT INTO Resources (Name, ResourceTypeID, Description, Location, IsActive, IsDeleted)
SELECT 'Mini Event Hall', ResourceTypeID, 'Smaller event space for gatherings, seats 50', 'Block H, Level 1', 1, 0
FROM ResourceTypes WHERE TypeName = 'Event Hall';
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
