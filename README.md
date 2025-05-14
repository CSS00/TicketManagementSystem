# Ticket Management System

This is a `.NET` Web API project designed to provide functionality for managing users, events, tickets, and reservations — similar to Ticketmaster.


## Features

- User, Event, Ticket, and Reservation Management
- User registration and JWT-based authentication and Authorization
- Admin API with role based authorization.
- In-Memory Database (EF Core)
- Swagger UI for API Testing

## Technologies Used

- **Framework**: .NET 9.0
- **Language**: C#
- **Database**: In-Memory DB
- **Auth**: JWT (System.IdentityModel.Tokens.Jwt)
- **Tools&Libraries**: Entity Framework Core, Newtonsoft.Json, Swagger/Swashbuckle.AspNetCore

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- VSCode or any preferred IDE

### Running the Project

1. Clone the repository:
    ```bash
    git clone https://github.com/CSS00/TicketManagementSystem.git
    cd TicketManagementSystem
    ```
2. Restore dependencies:
    ```bash
    dotnet restore
    ```
3. Build:
    ```bash
    dotnet build
    ```
4. Run the application:
    ```bash
    dotnet run
    ```

### API Documentation

The API documentation is available via Swagger. Once the application is running, navigate to:
```
http://localhost:<port>/swagger
```

### Authentication

All secured endpoints require a **JWT token**.  
Use the `/api/auth/login` to authenticate and receive a token. You can use these pre-created accounts below, or got to `/api/user/register` to register a new regular user.
Copy the token and click the **Authorize** button in Swagger, then paste the token in the pop-up window.
```
Admin
{
  "email":"sl@gmail.com",
  "password":"67890"
}

Regular user
{
  "email":"abc@gmail.com",
  "password":"12345"
}
```

## Project Details

### Directories

- **Controllers**: Handles incoming HTTP requests.
- **Services**: Contains business logic (or mocks of services for which implementation is not part of this project).
- **Models**: Defines data structures.

### Configuration

In appsettings.json or through environment variables:
```
"Jwt": {
  "Key": "your-very-secure-secret"
}
```
For security, move this secret to a secure location in production (e.g., Azure Key Vault, environment variables, etc).

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

## Contact

For any inquiries, please contact [congshanlv@gmail.com].  