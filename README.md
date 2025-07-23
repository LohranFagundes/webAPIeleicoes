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

[http://localhost:5000/swagger](http://localhost:5000/swagger)

### Base URL
- **Development**: `http://localhost:5000`
- **Authentication**: All protected endpoints require JWT Bearer token
- **Response Format**: All responses follow the `ApiResponse<T>` wrapper format

---

## 🔐 Authentication Endpoints

### Admin Login
```http
POST /api/auth/admin/login
Content-Type: application/json

{
  "email": "admin@election-system.com",
  "password": "admin123"
}
```
**Response**: JWT token valid for 59 minutes

### Voter Login
```http
POST /api/auth/voter/login
Content-Type: application/json

{
  "email": "voter@example.com",
  "password": "password"
}
```
**Response**: JWT token valid for 10 minutes

### Logout
```http
POST /api/auth/logout
Authorization: Bearer <token>
```

---

## 🗳️ Election Management (Admin Only)

### List Elections
```http
GET /api/election?page=1&limit=10&status=active&type=internal&search=presidential
Authorization: Bearer <admin-token>
```

### Get Election Details
```http
GET /api/election/{id}
Authorization: Bearer <admin-token>
```

### Create Election
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

### Update Election
```http
PUT /api/election/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json
```

### Update Election Status
```http
PATCH /api/election/{id}/status
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "status": "active"
}
```

### Delete Election
```http
DELETE /api/election/{id}
Authorization: Bearer <admin-token>
```

### Get Election Results
```http
GET /api/election/{id}/results
Authorization: Bearer <admin-token>
```

### Export Election Results
```http
GET /api/election/{id}/export?format=pdf
Authorization: Bearer <admin-token>
```

---

## 👥 Candidate Management (Admin Only)

### List Candidates
```http
GET /api/candidate?page=1&limit=10&positionId=1&search=maria
Authorization: Bearer <admin-token>
```

### Get Candidate Details
```http
GET /api/candidate/{id}
Authorization: Bearer <admin-token>
```

### Get Candidates by Position (Public)
```http
GET /api/candidate/position/{positionId}
```

### Get Candidates with Vote Counts
```http
GET /api/candidate/position/{positionId}/with-votes
Authorization: Bearer <admin-token>
```

### Create Candidate
```http
POST /api/candidate
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "Maria Santos",
  "number": "10",
  "description": "Experienced candidate",
  "biography": "15 years of leadership experience",
  "positionId": 1,
  "orderPosition": 1,
  "isActive": true
}
```

### Update Candidate
```http
PUT /api/candidate/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json
```

### Delete Candidate
```http
DELETE /api/candidate/{id}
Authorization: Bearer <admin-token>
```

### Upload Candidate Photo
```http
POST /api/candidate/{id}/upload-photo
Authorization: Bearer <admin-token>
Content-Type: multipart/form-data

Form Data: photo (file, max 5MB, JPG/PNG/GIF)
```

### Reorder Candidates
```http
PUT /api/candidate/position/{positionId}/reorder
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "candidateOrders": [
    { "candidateId": 1, "orderPosition": 1 },
    { "candidateId": 2, "orderPosition": 2 }
  ]
}
```

---

## 📋 Position Management (Admin Only)

### List Positions
```http
GET /api/position?page=1&limit=10&electionId=1&search=president
Authorization: Bearer <admin-token>
```

### Get Position Details
```http
GET /api/position/{id}
Authorization: Bearer <admin-token>
```

### Get Position with Candidates
```http
GET /api/position/{id}/with-candidates
Authorization: Bearer <admin-token>
```

### Get Positions by Election (Public)
```http
GET /api/position/election/{electionId}
```

### Create Position
```http
POST /api/position
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "title": "President",
  "description": "Chief executive position",
  "electionId": 1,
  "maxCandidates": 10,
  "maxVotesPerVoter": 1,
  "orderPosition": 1,
  "isActive": true
}
```

### Update Position
```http
PUT /api/position/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json
```

### Delete Position
```http
DELETE /api/position/{id}
Authorization: Bearer <admin-token>
```

### Reorder Positions
```http
PUT /api/position/election/{electionId}/reorder
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "positionOrders": [
    { "positionId": 1, "orderPosition": 1 },
    { "positionId": 2, "orderPosition": 2 }
  ]
}
```

---

## 👤 Voter Management

### List Voters (Admin Only)
```http
GET /api/voter?page=1&limit=10&isActive=true&isVerified=true&search=john
Authorization: Bearer <admin-token>
```

### Get Voter Details (Admin Only)
```http
GET /api/voter/{id}
Authorization: Bearer <admin-token>
```

### Get Voter Profile (Voter Only)
```http
GET /api/voter/profile
Authorization: Bearer <voter-token>
```

### Get Voter Statistics (Admin Only)
```http
GET /api/voter/statistics
Authorization: Bearer <admin-token>
```

### Create Voter (Admin Only)
```http
POST /api/voter
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@example.com",
  "cpf": "12345678901",
  "password": "securepassword",
  "voteWeight": 1.0,
  "isActive": true
}
```

### Update Voter (Admin Only)
```http
PUT /api/voter/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json
```

### Update Voter Profile (Voter Only)
```http
PUT /api/voter/profile
Authorization: Bearer <voter-token>
Content-Type: application/json

{
  "name": "João Silva Santos",
  "email": "joao.silva@example.com"
}
```

### Delete Voter (Admin Only)
```http
DELETE /api/voter/{id}
Authorization: Bearer <admin-token>
```

### Verify Email (Public)
```http
POST /api/voter/verify-email
Content-Type: application/json

{
  "email": "voter@example.com",
  "verificationCode": "ABC123"
}
```

### Send Verification Email (Admin Only)
```http
POST /api/voter/{id}/send-verification
Authorization: Bearer <admin-token>
```

### Change Password (Voter Only)
```http
POST /api/voter/change-password
Authorization: Bearer <voter-token>
Content-Type: application/json

{
  "currentPassword": "oldpassword",
  "newPassword": "newpassword"
}
```

### Reset Password (Public)
```http
POST /api/voter/reset-password
Content-Type: application/json

{
  "email": "voter@example.com"
}
```

---

## ⚡ Voting System

### Check Voting Eligibility
```http
GET /api/voting/can-vote/{electionId}
Authorization: Bearer <voter-token>
```

### Cast Vote
```http
POST /api/voting/cast
Authorization: Bearer <voter-token>
Content-Type: application/json

{
  "electionId": 1,
  "votes": [
    {
      "positionId": 1,
      "candidateId": 5,
      "voteType": "candidate"
    }
  ]
}
```

### Get Voting History (Voter Only)
```http
GET /api/voting/history
Authorization: Bearer <voter-token>
```

### Validate Vote Receipt (Public)
```http
POST /api/voting/validate-receipt
Content-Type: application/json

{
  "receiptCode": "VR-ABC123-XYZ789",
  "electionId": 1
}
```

---

## 📧 Email System (Admin Only)

### Send Email
```http
POST /api/email/send
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "to": "voter@example.com",
  "subject": "Election Notification",
  "body": "Your election notification message",
  "isHtml": true
}
```

### Send Bulk Email
```http
POST /api/email/send-bulk
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "subject": "Election Announcement",
  "body": "Election announcement message",
  "recipients": ["voter1@example.com", "voter2@example.com"],
  "isHtml": true
}
```

### Send Template Email
```http
POST /api/email/send-template
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "templateName": "election-reminder",
  "to": "voter@example.com",
  "templateData": {
    "voterName": "João Silva",
    "electionTitle": "Presidential Election 2024"
  }
}
```

### Get Email History
```http
GET /api/email/history?page=1&limit=10
Authorization: Bearer <admin-token>
```

### Validate Email Configuration
```http
POST /api/email/validate-config
Authorization: Bearer <admin-token>
```

---

## 📊 Reports & Audit (Admin Only)

### Get Audit Logs
```http
GET /api/report/audit-logs?page=1&limit=10&startDate=2024-01-01&endDate=2024-12-31&action=vote_cast
Authorization: Bearer <admin-token>
```

### Get Specific Audit Log
```http
GET /api/report/audit-logs/{id}
Authorization: Bearer <admin-token>
```

### Export Audit Report
```http
GET /api/report/audit-export?format=csv&startDate=2024-01-01&endDate=2024-12-31
Authorization: Bearer <admin-token>
```

### Get Real-time Activity
```http
GET /api/report/real-time
Authorization: Bearer <admin-token>
```

### Cleanup Old Logs
```http
POST /api/report/cleanup-old-logs
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "daysToKeep": 90
}
```

---

## 🏥 System Health

### Health Check
```http
GET /health
```

### API Information
```http
GET /
```

---

## 📋 Response Format

All API endpoints return responses in the following standardized format:

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    // Response data here
  },
  "errors": null
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error message description",
  "data": null,
  "errors": [
    "Detailed error message 1",
    "Detailed error message 2"
  ]
}
```

### Paginated Response
```json
{
  "success": true,
  "message": "",
  "data": {
    "items": [
      // Array of items
    ],
    "totalItems": 50,
    "totalPages": 5,
    "currentPage": 1,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "errors": null
}
```

---

## 📊 HTTP Status Codes

- **200 OK**: Request successful
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Authentication required or invalid token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict (e.g., duplicate email)
- **422 Unprocessable Entity**: Validation errors
- **500 Internal Server Error**: Server error

---

## 🔑 Authentication Headers

For all protected endpoints, include the JWT token in the Authorization header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Expiration
- **Admin tokens**: 59 minutes
- **Voter tokens**: 10 minutes

### Token Claims
- `user_id`: User identifier
- `role`: User role (admin/voter)
- `iat`: Issued at timestamp
- `exp`: Expiration timestamp
- `jti`: Unique token identifier

---

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
