# ToolRental API

**Peer-to-Peer Tool Rental Platform** backend built with **.NET 8** and **ASP.NET Core**.

This project was created to practice **Clean Architecture**, handling **complex business logic**, and managing **database relationships**. My goal was to build a system similar to Airbnb, but for renting tools.

## Key Features

### Identity & Security
- **Authentication:** Users can register and log in safely using **ASP.NET Core Identity** and **JWT Tokens**.
- **Data Protection:** I implemented a separation between public and private data:
  - **Public Profile:** Shows only necessary info (Name, Rating) to other users.
  - **Private Profile:** Owners can see their sensitive data (Balance, Email).
  - **Profile Updates:** Profile changes are validated against the user's token to prevent unauthorized modifications.

### Wallet & Transaction System
- **Virtual Wallet:** Every user has a `Balance` and a `LockedBalance`.
- **Booking Logic:** When a user rents a tool, the price + deposit is moved to the **Locked Balance**. This amount is only released to the owner after the tool is returned successfully.
- **Transaction History:** The system tracks every deposit, payment, and refund in a transaction log.

### Rating System
- **System**: Users can easily rate other users for better reliability.
- **Validation:** The system prevents users from rating a user multiple times.

---

## Tech Stack

- **Framework:** .NET 8 Web API
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core
- **Validation:** FluentValidation
- **Architecture:** Service Layer pattern with Result Pattern.
- **Documentation:** Swagger / OpenAPI

---

## API Endpoints (Overview)

The API is documented via **Swagger/OpenAPI**. Below are the main endpoints of the application:

| Feature | Method | Endpoint | Description |
| :--- | :--- | :--- | :--- |
| **Auth** | POST | /api/auth/register | Create a new user account. |
| **Auth** | POST | /api/auth/login | Returns a **Bearer Token** required for other endpoints. |
| **Users** | GET | /api/users/profile | **(Auth Required)** Get own details. |
| **Users** | PUT | /api/users/profile | **(Auth Required)** Update own details. |
| **Users** | GET | /api/users/{id} | **(Auth Required)** View another user's public profile and rating stats. |
| **Transactions** | GET | /api/transactions | **(Auth Required)** List personal financial history. |
| **Bookings** | POST | /api/bookings | **(Auth Required)** Create a booking and lock renter's funds. |

> **Note on Testing:** Most endpoints require a valid JWT Token. If the Swagger UI does not have the "Authorize" button enabled, I recommend using **[Bruno](https://www.usebruno.com/)** (an open-source API client) to test the API. Include the token in the `Authorization` header: `Bearer <your-token>`.

---

## Architecture Highlights

### The Result Pattern
Instead of throwing Exceptions for logic errors (e.g., "User not found"), I use a generic `Result<T>`. This makes error handling cleaner.

### DTO Separation
I separated the Database Entities from the API responses:
- `User` (Entity): Contains user's data.
- `PublicUserResponse` (DTO): Safe for public viewing.
- `MyProfileResponse` (DTO): Detailed view for the owner.

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server

### Installation Guide

### 1. Clone the repository
```bash
git clone https://github.com/TakacsBalazs/RentalAPI.git
```

### 2. Configure Database
Open `appsettings.json` and update the `DefaultConnection` string to point to your SQL Server instance.

### 3. Apply Migrations
Initialize the database schema:
```bash
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```
The API will be available at `https://localhost:[PORT]`. You can browse the endpoints at `/swagger`.