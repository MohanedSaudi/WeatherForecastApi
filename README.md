# 🌦️ Weather Forecast API - Clean Architecture

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoft-sql-server)
![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=c-sharp)
![License](https://img.shields.io/badge/License-MIT-green)

A  Weather Forecast API built with **.NET 8** and **Clean Architecture** principles, featuring JWT authentication with refresh tokens, SQL Server database, comprehensive testing, and advanced security measures.

---

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Technology Stack](#-technology-stack)
- [Project Structure](#-project-structure)
- [API Documentation](#-api-documentation)
- [Running Tests](#-running-tests)
- [Configuration](#-configuration)
- [Default Users](#-default-users)
- [Security Features](#-security-features)

---

## ✨ Features

### Core Architecture
- ✅ **Clean Architecture** with vertical slice organization (feature-based folders)
- ✅ **CQRS Pattern** with MediatR for separation of reads and writes
- ✅ **Result Pattern** - Type-safe error handling without exceptions in Domain/Application layers
- ✅ **Rich Domain Models** with encapsulation and factory methods
- ✅ **Repository Pattern** with specific repositories (no generic repository)
- ✅ **Unit of Work** pattern for transaction management

### Security & Authentication
- ✅ **JWT Authentication** with short-lived access tokens 
- ✅ **Refresh Tokens** stored in HttpOnly cookies 
- ✅ **Role-Based Authorization** (Admin, Premium, User)
- ✅ **CSRF Protection** with Anti-Forgery tokens
- ✅ **XSS Protection** with HttpOnly cookies and secure headers
- ✅ **Password Hashing** with BCrypt 
- ✅ **Rate Limiting** by IP address to prevent abuse
- ✅ **Security Headers** (HSTS, CSP, X-Frame-Options, X-Content-Type-Options)

### Database & Persistence
- ✅ **SQL Server** with Entity Framework Core 8
- ✅ **EF Core Migrations** for schema versioning
- ✅ **Seed Data** - 3 pre-configured users (Admin, Premium, Regular)
- ✅ **Auto-Migration** on application startup
- ✅ **Connection Resilience** with retry logic
- ✅ **Audit Tracking** - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

### Performance & Resilience
- ✅ **In-Memory Caching** with configurable expiration
- ✅ **Response Compression** (Brotli & Gzip)
- ✅ **Retry Policy** with exponential backoff (Polly)
- ✅ **Circuit Breaker** pattern for external API calls
- ✅ **Timeout Policy** for HTTP requests
- ✅ **AsNoTracking** queries for read operations

### Observability & Logging
- ✅ **Structured Logging** with Serilog
- ✅ **Multiple Sinks** - Console, File, Seq (optional)
- ✅ **Request/Response Logging** with correlation IDs
- ✅ **Performance Monitoring** - Log slow requests (>500ms)
- ✅ **Health Checks** endpoint

### Validation & Error Handling
- ✅ **FluentValidation** for request validation
- ✅ **Pipeline Behaviors** - Validation, Logging, Performance, Caching
- ✅ **Global Exception Handling** with ProblemDetails (RFC 7807)
- ✅ **Consistent Error Responses** across all endpoints

### Testing
- ✅ **Unit Tests** - Domain entities, Application handlers, Infrastructure services
- ✅ **Integration Tests** - API endpoints, Database operations
- ✅ **Architecture Tests** - Layer dependencies, Naming conventions

---

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer                            │
│  Controllers, Middleware, Program.cs                    │
│  (Presentation Logic)                                   │
└────────────────────┬───────────────────────────────────┘
                     │ Depends on
┌────────────────────▼───────────────────────────────────┐
│               Application Layer                         │
│  Commands, Queries, Handlers, Validators, DTOs          │
│  (Business Logic - Use Cases)                           │
└────────────────────┬───────────────────────────────────┘
                     │ Depends on
┌────────────────────▼───────────────────────────────────┐
│                 Domain Layer                            │
│  Entities, Value Objects, Errors, Interfaces            │
│  (Enterprise Business Rules - Core Logic)               │
└─────────────────────────────────────────────────────────┘
                     ▲
┌────────────────────┴───────────────────────────────────┐
│              Infrastructure Layer                       │
│  EF Core, Repositories, External Services, JWT          │
│  (External Concerns - Database, APIs)                   │
└─────────────────────────────────────────────────────────┘
```

### Design Patterns Used
- **CQRS** - Separate read and write operations
- **Result Pattern** - Type-safe error handling
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Factory Pattern** - Rich domain model creation
- **Pipeline Behavior** - Cross-cutting concerns

---

## 🛠️ Technology Stack

### Backend Framework
- **.NET 8.0** - Latest LTS version
- **ASP.NET Core Web API** - RESTful API framework
- **C# 12** - Latest language features

### Database & ORM
- **SQL Server 2022** / **LocalDB** - Relational database
- **Entity Framework Core 8** - ORM
- **EF Core Migrations** - Database versioning

### Libraries & Packages
- **MediatR** (12.x) - CQRS implementation
- **FluentValidation** (11.x) - Request validation
- **BCrypt.NET** (4.x) - Password hashing
- **Serilog** (3.x) - Structured logging
- **Polly** (8.x) - Resilience and transient-fault-handling
- **Swashbuckle** (6.x) - OpenAPI/Swagger documentation

### Testing
- **xUnit** (2.6) - Test framework
- **FluentAssertions** (6.12) - Fluent assertions
- **Moq** (4.20) - Mocking framework
- **NetArchTest** (1.3) - Architecture testing

---

## 📁 Project Structure

```
WeatherForecastApi/
│
├── src/
│   ├── WeatherApi.Domain/                      # Enterprise Business Rules
│   │   ├── Common/
│   │   │   ├── BaseEntity/
│   │   │   │   ├── ImmutableEntity.cs         # Base for immutable entities
│   │   │   │   ├── MutableEntity.cs           # Base for mutable entities
│   │   │   │   └── IAuditableEntity.cs        # Audit interface
│   │   │   ├── Result/
│   │   │   │   ├── Result.cs                  # Result pattern implementation
│   │   │   │   ├── Error.cs                   # Error types
│   │   │   │   └── ErrorType.cs               # Error categories
│   │   │   └── Interfaces/
│   │   │       └── IUnitOfWork.cs             # Unit of Work interface
│   │   ├── Users/
│   │   │   ├── User.cs                        # User entity (Rich Domain Model)
│   │   │   ├── Email.cs                       # Email value object
│   │   │   ├── Password.cs                    # Password value object
│   │   │   ├── UserErrors.cs                  # User-specific errors
│   │   │   └── IUserRepository.cs             # User repository interface
│   │   ├── RefreshTokens/
│   │   │   ├── RefreshToken.cs                # Refresh token entity
│   │   │   ├── RefreshTokenErrors.cs          # Token errors
│   │   │   └── IRefreshTokenRepository.cs     # Token repository interface
│   │   ├── Weather/
│   │   │   ├── WeatherCache.cs                # Weather cache entity
│   │   │   ├── WeatherErrors.cs               # Weather errors
│   │   │   └── IWeatherCacheRepository.cs     # Weather repository interface
│   │   └── Enums/
│   │       └── UserRole.cs                    # User roles (Admin, Premium, User)
│   │
│   ├── WeatherApi.Application/                # Application Business Rules
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs      # FluentValidation pipeline
│   │   │   │   ├── LoggingBehavior.cs         # Request/Response logging
│   │   │   │   ├── PerformanceBehavior.cs     # Performance monitoring
│   │   │   │   └── CachingBehavior.cs         # Caching pipeline
│   │   │   ├── Interfaces/
│   │   │   │   ├── IJwtService.cs             # JWT service interface
│   │   │   │   ├── IWeatherApiClient.cs       # Weather API interface
│   │   │   │   ├── ICacheService.cs           # Cache service interface
│   │   │   │   └── ICurrentUserService.cs     # Current user context
│   │   │   └── Constants/
│   │   │       └── Policies.cs                # Authorization policies
│   │   ├── Users/
│   │   │   ├── Register/
│   │   │   │   ├── RegisterCommand.cs         # Registration command
│   │   │   │   ├── RegisterCommandHandler.cs  # Command handler
│   │   │   │   └── RegisterCommandValidator.cs # Validation rules
│   │   │   ├── Login/
│   │   │   │   ├── LoginCommand.cs            # Login command
│   │   │   │   ├── LoginCommandHandler.cs     # Command handler
│   │   │   │   └── LoginCommandValidator.cs   # Validation rules
│   │   │   ├── RefreshToken/
│   │   │   │   ├── RefreshTokenCommand.cs     # Refresh token command
│   │   │   │   └── RefreshTokenCommandHandler.cs
│   │   │   ├── RevokeToken/
│   │   │   │   ├── RevokeTokenCommand.cs      # Revoke token command
│   │   │   │   └── RevokeTokenCommandHandler.cs
│   │   │   └── DTOs/
│   │   │       └── AuthResponse.cs            # Authentication response DTO
│   │   ├── Weather/
│   │   │   ├── GetWeather/
│   │   │   │   ├── GetWeatherQuery.cs         # Weather query
│   │   │   │   ├── GetWeatherQueryHandler.cs  # Query handler
│   │   │   │   └── GetWeatherQueryValidator.cs # Validation rules
│   │   │   └── DTOs/
│   │   │       └── WeatherDto.cs              # Weather response DTO
│   │   └── DependencyInjection.cs             # Application services registration
│   │
│   ├── WeatherApi.Infrastructure/             # External Concerns
│   │   ├── Common/
│   │   │   └── Persistence/
│   │   │       ├── ApplicationDbContext.cs    # EF Core DbContext
│   │   │       ├── DbInitializer.cs           # Database initialization
│   │   │       └── UnitOfWork.cs              # Unit of Work implementation
│   │   ├── Users/
│   │   │   ├── UserConfiguration.cs           # EF Core entity configuration
│   │   │   └── UserRepository.cs              # User repository implementation
│   │   ├── RefreshTokens/
│   │   │   ├── RefreshTokenConfiguration.cs   # EF Core configuration
│   │   │   └── RefreshTokenRepository.cs      # Token repository
│   │   ├── Weather/
│   │   │   ├── WeatherCacheConfiguration.cs   # EF Core configuration
│   │   │   ├── WeatherCacheRepository.cs      # Cache repository
│   │   │   ├── WeatherApiClient.cs            # HTTP client for weather API
│   │   │   └── WeatherApiSettings.cs          # Configuration settings
│   │   ├── Authentication/
│   │   │   ├── JwtService.cs                  # JWT token generation
│   │   │   └── CurrentUserService.cs          # Current user context
│   │   ├── Caching/
│   │   │   └── CacheService.cs                # Memory cache implementation
│   │   ├── Resilience/
│   │   │   └── ResiliencePolicies.cs          # Polly policies (Retry, Circuit Breaker)
│   │   └── DependencyInjection.cs             # Infrastructure services registration
│   │
│   └── WeatherApi.API/                        # Presentation Layer
│       ├── Common/
│       │   ├── Middleware/
│       │   │   ├── GlobalExceptionMiddleware.cs   # Global exception handler
│       │   │   ├── SecurityHeadersMiddleware.cs   # Security headers
│       │   │   └── AntiForgeryMiddleware.cs       # CSRF protection
│       │   └── Extensions/
|       |       |── MiddlewareExtenstions.cs
│       │       ├── RateLimitingExtensions.cs      # Rate limiting setup
│       │       └── SerilogExtensions.cs           # Serilog configuration
|       |       |── DependencyInjection.cs
│       ├── Controllers/
│       │   ├── AuthController.cs              # Authentication endpoints
│       │   └── WeatherController.cs           # Weather endpoints
│       ├── Program.cs                         # Application entry point
│       ├── appsettings.json                   # Production configuration
│       └── appsettings.Development.json       # Development configuration
│
├── tests/
│   ├── WeatherApi.UnitTests/                  # Unit Tests
│   │   ├── Domain/
│   │   │   ├── Users/
│   │   │   │   ├── UserTests.cs              # User entity tests
│   │   │   │   ├── EmailTests.cs             # Email value object tests
│   │   │   │   └── PasswordTests.cs          # Password value object tests
│   │   │   └── RefreshTokens/
│   │   │       └── RefreshTokenTests.cs      # Refresh token tests
│   │   ├── Application/
│   │   │   ├── Users/
│   │   │   │   ├── RegisterCommandHandlerTests.cs
│   │   │   │   └── LoginCommandHandlerTests.cs
│   │   │   └── Weather/
│   │   │       └── GetWeatherQueryHandlerTests.cs
│   │   └── Infrastructure/
│   │       └── Services/
│   │           └── JwtServiceTests.cs        # JWT service tests
│   │
│   ├── WeatherApi.IntegrationTests/          # Integration Tests
│   │   ├── Common/
│   │   │   ├── CustomWebApplicationFactory.cs # Test server factory
│   │   │   └── IntegrationTestBase.cs        # Base test class
│   │   ├── Controllers/
│   │   │   ├── AuthControllerTests.cs        # Auth API tests
│   │   │   └── WeatherControllerTests.cs     # Weather API tests
│   │   └── Repositories/
│   │       └── UserRepositoryTests.cs        # Repository tests
│   │
│   └── WeatherApi.ArchitectureTests/         # Architecture Tests
│       ├── ArchitectureTests.cs              # Layer dependency tests
│       ├── DomainTests.cs                    # Domain purity tests
│       ├── ApplicationTests.cs               # Application tests
│       └── NamingConventionTests.cs          # Naming convention tests
│
├── WeatherApi.API.http                        # HTTP request examples
├── README.md                                  # This file
└── WeatherForecastApi.sln                    # Solution file
```

---

## 📡 API Documentation

### Base URL
```
Development: https://localhost:7060 or http://localhost:5053
```

### Authentication Endpoints

#### 1. Register New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!@#"
}
```

**Success Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "",
  "username": "johndoe",
  "email": "john@example.com",
  "role": "User"
}
```

**Error Responses:**
- `400 Bad Request` - Validation errors (weak password, invalid email)
- `409 Conflict` - Email already exists

#### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@weatherapi.com",
  "password": "Admin123!@#"
}
```

**Success Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "",
  "username": "admin",
  "email": "admin@weatherapi.com",
  "role": "Admin"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid credentials or inactive user

#### 3. Refresh Token
```http
POST /api/auth/refresh-token
```
*Note: Refresh token is automatically sent via HttpOnly cookie*

**Success Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "",
  "username": "admin",
  "email": "admin@weatherapi.com",
  "role": "Admin"
}
```

#### 4. Logout (Revoke Token)
```http
POST /api/auth/revoke-token
Authorization: Bearer {accessToken}
```

**Success Response (204 No Content)**

### Weather Endpoints

#### 5. Get Weather for City
```http
GET /api/weather?city=London
Authorization: Bearer {accessToken}
```

**Success Response (200 OK):**
```json
{
  "city": "London",
  "date": "2024-01-15T10:30:00Z",
  "temperatureC": 15,
  "temperatureF": 59,
  "summary": "Mild",
  "description": "Partly cloudy with mild temperatures",
  "humidity": 65,
  "windSpeed": 12.5
}
```

**Error Responses:**
- `400 Bad Request` - Invalid city name
- `401 Unauthorized` - Missing or invalid token

#### 6. Bulk Weather Request (Premium Users Only)
```http
POST /api/weather/bulk
Authorization: Bearer {accessToken}
Content-Type: application/json

["London", "Paris", "Tokyo", "NewYork"]
```

**Success Response (200 OK):**
```json
[
  {
    "city": "London",
    "temperatureC": 15,
    ...
  },
  {
    "city": "Paris",
    "temperatureC": 18,
    ...
  }
]
```

**Error Responses:**
- `403 Forbidden` - User doesn't have Premium or Admin role

### Health Check

#### 7. Health Check
```http
GET /health
```

**Success Response (200 OK):**
```json
{
  "status": "Healthy"
}
```

---

## 🧪 Running Tests

### Run All Tests
```bash
# Run all test projects
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run Specific Test Projects
```bash
# Unit tests only (Domain, Application, Infrastructure)
dotnet test tests/WeatherApi.UnitTests

# Integration tests only (API endpoints, Repositories)
dotnet test tests/WeatherApi.IntegrationTests

# Architecture tests only (Layer dependencies, Naming conventions)
dotnet test tests/WeatherApi.ArchitectureTests
```

### Run Specific Test
```bash
# Run a specific test by name
dotnet test --filter "FullyQualifiedName~UserTests.Create_WithValidData_ShouldSucceed"

# Run all tests in a specific class
dotnet test --filter "FullyQualifiedName~UserTests"
```

### Test Coverage Summary
- **Unit Tests**: 35+ tests covering Domain, Application, and Infrastructure layers
- **Integration Tests**: 20+ tests covering API endpoints and database operations
- **Architecture Tests**: 20+ tests ensuring Clean Architecture principles
- **Total Coverage**: 90%+ code coverage

---

## ⚙️ Configuration

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WeatherApiDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForDevelopment!",
    "Issuer": "WeatherApi",
    "Audience": "WeatherApiUsers"
  },
  "WeatherApi": {
    "UseFakeData": true
  }
}
```

### Environment Variables

You can override configuration using environment variables:

```bash
# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=WeatherApiDb;..."
dotnet run --project src/WeatherApi.API

# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=localhost;Database=WeatherApiDb;..."
dotnet run --project src/WeatherApi.API
```

---

## 👥 Default Users

The application comes with 3 pre-seeded users for testing:

| Email | Password | Role | Permissions |
|-------|----------|------|-------------|
| `admin@weatherapi.com` | `Admin123!@#` | **Admin** | All permissions including bulk requests |
| `premium@weatherapi.com` | `Premium123!@#` | **Premium** | Weather requests + bulk requests |
| `test@weatherapi.com` | `Test123!@#` | **User** | Basic weather requests only |

### Password Requirements
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit
- At least 1 special character

---

## 🔒 Security Features

### Implemented Security Measures

1. **Authentication & Authorization**
   - JWT tokens with HMAC-SHA256 signing
   - Role-based access control (Admin, Premium, User)
   - Short-lived access tokens 
   - Long-lived refresh tokens (revocable)

2. **Password Security**
   - BCrypt hashing
   - Password complexity validation
   - Secure password comparison

3. **CSRF Protection**
   - Anti-forgery tokens
   - SameSite=Strict cookies
   - HttpOnly cookies for refresh tokens

4. **XSS Protection**
   - Content Security Policy headers
   - X-Content-Type-Options: nosniff
   - X-Frame-Options: DENY

5. **Rate Limiting**
   - IP-based throttling (200 requests/minute global)
   - Per-user limits (100 requests/minute)
   - Configurable rate limit windows

6. **Security Headers**
   - HSTS (HTTP Strict Transport Security)
   - CSP (Content Security Policy)
   - X-XSS-Protection
   - Referrer-Policy: strict-origin-when-cross-origin

7. **Data Protection**
   - SQL injection prevention (EF Core parameterized queries)
   - Input validation with FluentValidation
   - Output encoding

### Security Best Practices

⚠️ **IMPORTANT**: Before deploying to production:

1. ✅ Change the JWT secret key in `appsettings.json`
2. ✅ Use HTTPS in production (never HTTP)
3. ✅ Store secrets in environment variables or Azure Key Vault
4. ✅ Enable SQL Server encryption
5. ✅ Use strong database passwords
6. ✅ Regularly update NuGet packages
7. ✅ Monitor logs for suspicious activity
8. ✅ Enable database backups

---

## 🤝 Troubleshooting

### Common Issues

#### Issue 1: "Cannot connect to database"
**Solution**: Verify SQL Server/LocalDB is running:
```bash
# Check LocalDB
sqllocaldb info mssqllocaldb

# Start LocalDB if stopped
sqllocaldb start mssqllocaldb

# Or update connection string in appsettings.Development.json
```

#### Issue 2: "Migration not found"
**Solution**: Create and apply migration:
```bash
dotnet ef migrations add InitialCreate --project src/WeatherApi.Infrastructure --startup-project src/WeatherApi.API
dotnet ef database update --project src/WeatherApi.Infrastructure --startup-project src/WeatherApi.API
```

#### Issue 3: "Port already in use"
**Solution**: Change port in `launchSettings.json` or kill process:
```bash
# Windows
netstat -ano | findstr :7060
taskkill /PID <PID> /F

# Linux/macOS
lsof -ti:7060 | xargs kill
```

#### Issue 4: "401 Unauthorized" on API calls
**Solution**: 
1. Login first to get access token
2. Add `Authorization: Bearer {token}` header to requests
3. Check token hasn't expired (15 min lifetime)

---
