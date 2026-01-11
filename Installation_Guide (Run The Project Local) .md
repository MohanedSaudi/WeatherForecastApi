
## 📋 Prerequisites

Before running this project, ensure you have the following installed:

### Required
- ✅ [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Version 8.0 or later)
- ✅ [SQL Server 2022](https://www.microsoft.com/sql-server/sql-server-downloads) **OR** SQL Server LocalDB
- ✅ [Git](https://git-scm.com/downloads)

### Recommended
- ✅ [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+) **OR** [VS Code](https://code.visualstudio.com/)
- ✅ [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- ✅ [Postman](https://www.postman.com/downloads/) or similar tool for API testing

### Verify Installation

```bash
# Check .NET version (should be 8.0.x or higher)
dotnet --version

# Check SQL Server (LocalDB)
sqllocaldb info

# Check Git
git --version
```

---

## 🚀 Getting Started

### Step 1: Clone the Repository

```bash
git clone https://github.com/MohanedSaudi/WeatherForecastApi.git
cd WeatherForecastApi
```

### Step 2: Update Connection String

**Option A: Using SQL Server LocalDB (Recommended for Development)**

Open `src/WeatherApi.API/appsettings.Development.json` and verify the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WeatherApiDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Option B: Using SQL Server Instance**

If you have SQL Server installed, update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WeatherApiDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Option C: Using SQL Server with Username/Password**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WeatherApiDb;User Id=sa;Password=YourPassword123!;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Step 3: Restore Dependencies

```bash
# Restore NuGet packages
dotnet restore
```

### Step 4: Install EF Core Tools (if not already installed)

```bash
# Install globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef

# Verify installation
dotnet ef --version
```

### Step 5: Create and Apply Database Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate --project src/WeatherApi.Infrastructure --startup-project src/WeatherApi.API

# Apply migration to database (creates database and tables)
dotnet ef database update --project src/WeatherApi.Infrastructure --startup-project src/WeatherApi.API
```

**Note:** The database will also auto-migrate when you run the application for the first time.

### Step 6: Build the Solution

```bash
# Build the entire solution
dotnet build

# Or build specific project
dotnet build src/WeatherApi.API/WeatherApi.API.csproj
```

### Step 7: Run the Application

```bash
# Run the API
dotnet run --project src/WeatherApi.API

# The API will start on:
# - HTTPS: https://localhost:7060
# - HTTP: http://localhost:5053
```

### Step 8: Access Swagger UI

Open your browser and navigate to:
- **Swagger UI**: https://localhost:7060/swagger/index.html or http://localhost:5053/swagger/index.html 
