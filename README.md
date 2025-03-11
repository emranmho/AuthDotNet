# Auth in .Net

A comprehensive authentication and authorization solution for .NET applications, featuring JWT token-based authentication, role-based authorization, and refresh token functionality.

## Features

- 🔐 **User Authentication**: Secure registration and login functionality
- 🎫 **JWT Token Authentication**: Secure API endpoints with JSON Web Tokens
- 🔄 **Refresh Token Support**: Maintain user sessions without frequent logins
- 👮 **Role-Based Authorization**: Control access to resources based on user roles
- 📝 **Scalar API Testing**: Built-in support for API testing with Scalar
- 🛡️ **Attribute-Based Security**: Secure endpoints with `[Authorize]` attribute

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- [Docker](https://www.docker.com/products/docker-desktop/) (for SQL Server container)
- [Docker Compose](https://docs.docker.com/compose/install/) (usually included with Docker Desktop)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/) (optional)

## Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/emranmho/AuthDotNet.git
   cd AuthDotNet
   ```

2. **Start the SQL Server container**

   ```bash
   docker-compose up -d
   ```

   This will start a SQL Server instance and initialize the AuthDb database.

3. **Restore dependencies and build the project**

   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run the application**

   ```bash
   dotnet run
   ```

   The API will be available at `https://localhost:5001`.

## Usage

### API Endpoints

The following endpoints are available:

#### Authentication

- **Register**: `POST /api/Auth/register`
  - Creates a new user account
  - Request body: `{ "userName": "string", "password": "string" }`

- **Login**: `POST /api/Auth/login`
  - Authenticates a user and returns access and refresh tokens
  - Request body: `{ "userName": "string", "password": "string" }`

- **Refresh Token**: `POST /api/Auth/refresh-token`
  - Generates a new access token using a refresh token
  - Request body: `{ "userId": "guid", "refreshToken": "string" }`

#### Authorization

- **Admin Check**: `GET /api/Auth/auth-check/admin`
  - Requires Admin role
  - Returns a success message if the user is authenticated and has Admin role

- **User Check**: `GET /api/Auth/auth-check`
  - Requires Customer role
  - Returns a success message if the user is authenticated

### Using the API with HTTP Client

You can use the included `.http` file to test the API endpoints:

1. Open the `AuthDotNet.http` file in an HTTP client (like Visual Studio's built-in HTTP client or REST Client extension for VS Code)
2. Execute the register request to create a new user
3. Execute the login request to get an access token and refresh token
4. Use the access token to access protected endpoints
5. Use the refresh token to get a new access token when the current one expires

Example:

```http
@AuthDotNet_HostAddress = https://localhost:5001
@Token = your_access_token_here

### Register a new user
POST {{AuthDotNet_HostAddress}}/api/Auth/register
Content-Type: application/json

{
  "userName": "testuser",
  "password": "password123"
}

### Login
POST {{AuthDotNet_HostAddress}}/api/Auth/login
Content-Type: application/json

{
  "userName": "testuser",
  "password": "password123"
}

### Access protected endpoint
GET {{AuthDotNet_HostAddress}}/api/Auth/auth-check
Authorization: Bearer {{Token}}
```

## Project Structure

- **Controllers/**: Contains API controllers
  - `AuthController.cs`: Handles authentication and authorization endpoints
- **Services/**: Contains business logic
  - `AuthService.cs`: Implements authentication and token generation
  - `IAuthService.cs`: Interface for the authentication service
- **Models/**: Contains data transfer objects
  - `UserDto.cs`: User registration and login model
  - `TokenResponseDto.cs`: Token response model
  - `RefreshTokenRequestDto.cs`: Refresh token request model
- **Entities/**: Contains database entities
  - `User.cs`: User entity with authentication properties
- **Data/**: Contains database context and migrations

## Security Considerations

- JWT tokens are signed with a secure key defined in `appsettings.json`
- Passwords are hashed using ASP.NET Core's `PasswordHasher`
- Refresh tokens have an expiry time to limit their validity
- Role-based authorization restricts access to sensitive endpoints
