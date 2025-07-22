# Election API .NET

This is a secure Election API System built with .NET 8 and MySQL, converted from the original PHP version. It provides a robust backend for managing elections, voters, candidates, and votes with a layered architecture.

## Architecture

The API follows a clean architecture with the following layers:

```
ElectionApi.Net/
├── Controllers/     # API endpoints and HTTP request handling
├── Models/          # Entity models and database entities
├── Services/        # Business logic and service layer
├── Data/            # Data access layer, repositories, and DbContext
├── DTOs/            # Data Transfer Objects for API requests/responses
└── Middleware/      # Cross-cutting concerns (CORS, logging, exception handling)
```

## Features

- **Authentication & Authorization**: JWT-based authentication for admins and voters
- **Election Management**: Create, update, and manage elections with different types and statuses
- **Voter Management**: Register and manage voter accounts with verification
- **Voting System**: Secure voting with audit trails and vote validation
- **Audit Logging**: Comprehensive audit logging for all system activities
- **Security**: Password hashing, input validation, and secure API design
- **Documentation**: Swagger/OpenAPI documentation with JWT authentication

## Technologies Used

- **.NET 8**: Latest version of .NET for high performance
- **Entity Framework Core**: ORM for database operations
- **MySQL**: Database with Pomelo MySQL provider
- **JWT Authentication**: Secure token-based authentication
- **AutoMapper**: Object mapping between DTOs and entities
- **FluentValidation**: Input validation and business rules
- **Serilog**: Structured logging with file and console outputs
- **BCrypt**: Password hashing for security
- **Swagger/OpenAPI**: API documentation and testing

## Getting Started

### Prerequisites

- .NET 8 SDK
- MySQL Database
- Visual Studio Code or Visual Studio

### Installation

1. **Clone and navigate to the project:**
   ```bash
   cd ElectionApi.Net
   ```

2. **Install dependencies:**
   ```bash
   dotnet restore
   ```

3. **Configure your database:**
   Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=election_system;User=root;Password=yourpassword;CharSet=utf8mb4;"
     }
   }
   ```

4. **Configure JWT settings:**
   Update the JWT settings in `appsettings.json`:
   ```json
   {
     "JwtSettings": {
       "SecretKey": "your-super-secret-key-that-should-be-at-least-32-characters-long",
       "Issuer": "ElectionApi",
       "Audience": "ElectionApiUsers",
       "ExpireMinutes": 60
     }
   }
   ```

5. **Run database migrations:**
   ```bash
   dotnet ef database update
   ```

6. **Run the application:**
   ```bash
   dotnet run
   ```

## API Documentation

### Swagger UI

For interactive API documentation and testing, access the Swagger UI at:

[http://localhost:5000/docs](http://localhost:5000/docs)

### Authentication Endpoints

#### Admin Login
```http
POST /api/auth/admin/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "password"
}
```

#### Voter Login
```http
POST /api/auth/voter/login
Content-Type: application/json

{
  "email": "voter@example.com",
  "password": "password"
}
```

#### Logout
```http
POST /api/auth/logout
Authorization: Bearer <token>
```

### Election Endpoints

#### Get Elections (Admin only)
```http
GET /api/election?page=1&limit=10&status=active
Authorization: Bearer <admin-token>
```

#### Get Election by ID (Admin only)
```http
GET /api/election/{id}
Authorization: Bearer <admin-token>
```

#### Create Election (Admin only)
```http
POST /api/election
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "title": "Presidential Election 2024",
  "description": "Annual presidential election",
  "electionType": "internal",
  "startDate": "2024-01-01T08:00:00Z",
  "endDate": "2024-01-01T18:00:00Z",
  "timezone": "America/Sao_Paulo",
  "allowBlankVotes": true,
  "allowNullVotes": false,
  "maxVotesPerVoter": 1,
  "votingMethod": "single_choice",
  "resultsVisibility": "after_election"
}
```

#### Update Election Status (Admin only)
```http
PATCH /api/election/{id}/status
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "status": "active"
}
```

### Health Check

Check API health status:
```http
GET /health
```

## Models

### Election Model
- **Id**: Unique identifier
- **Title**: Election title
- **Description**: Election description
- **ElectionType**: Type of election (internal, external, etc.)
- **Status**: Current status (draft, scheduled, active, completed, cancelled)
- **StartDate/EndDate**: Election period
- **Voting Configuration**: Various voting rules and settings

### Voter Model
- **Id**: Unique identifier
- **Name**: Full name
- **Email**: Email address (unique)
- **CPF**: Brazilian tax ID (unique)
- **IsActive/IsVerified**: Account status
- **VoteWeight**: Weight of vote (default 1.0)

### Vote Model
- **Id**: Unique identifier
- **VoteType**: Type of vote (candidate, blank, null)
- **VoteHash**: Encrypted vote hash for security
- **VoterId**: Reference to voter
- **ElectionId**: Reference to election
- **CandidateId**: Reference to candidate (if applicable)

## Security Features

1. **JWT Authentication**: Secure token-based authentication with different expiration times for admins (1 hour) and voters (5 minutes)
2. **Password Hashing**: BCrypt for secure password storage
3. **Input Validation**: FluentValidation for comprehensive input validation
4. **Audit Logging**: Complete audit trail of all system activities
5. **CORS Configuration**: Configurable CORS policies
6. **Exception Handling**: Global exception handling with secure error messages

## Logging

The API uses Serilog for structured logging with:
- **Console Logging**: Development-friendly console output
- **File Logging**: Daily rolling log files in the `logs/` directory
- **Request Logging**: Detailed HTTP request/response logging
- **Audit Logging**: Business event logging in the database

## Database Schema

The API uses the same database schema as the PHP version:
- `admins`: Administrator accounts
- `voters`: Voter accounts
- `elections`: Election definitions
- `positions`: Election positions/offices
- `candidates`: Candidates for positions
- `votes`: Cast votes
- `audit_logs`: System audit trail

## Deployment

### Development
```bash
dotnet run --environment Development
```

### Production
```bash
dotnet publish -c Release
dotnet ElectionApi.Net.dll --environment Production
```

### Docker (Optional)
Create a `Dockerfile` for containerized deployment:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ElectionApi.Net.csproj", "."]
RUN dotnet restore "./ElectionApi.Net.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ElectionApi.Net.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ElectionApi.Net.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ElectionApi.Net.dll"]
```

## Contributing

This API was converted from the original PHP version maintaining the same functionality and database structure while leveraging .NET 8's modern features and performance improvements.

## License

This project maintains the same MIT License as the original PHP version.# webAPIeleicoes
# webAPIeleicoes
