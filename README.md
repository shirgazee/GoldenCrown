# GoldenCrown

A simple financial API service built with ASP.NET Core (.NET 10) that provides user authentication and basic banking operations like deposits, transfers, and transaction history.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQL Server
- Swagger/OpenAPI

## Startup Instructions

### Prerequisites

- .NET 10 SDK
- SQL Server

### Configuration

1. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=GoldenCrown;User Id=goldencrown;Password=Aa123456;TrustServerCertificate=True"
     }
   }
   ```

2. Apply database migrations:
   ```bash
   cd GoldenCrown
   dotnet ef database update
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).
Swagger UI is available at `/swagger`.

### Seed Data

The database is seeded with two test users:

| Login | Password | Name           | Balance |
|-------|----------|----------------|---------|
| admin | admin    | Administrator  | 1000.00 |
| user  | user     | Regular User   | 500.00  |

---

## API Reference

### Authentication

#### Register
```
POST /api/user/register
```
**Request Body:**
```json
{
  "login": "string",    // min 3 characters
  "name": "string",
  "password": "string"  // min 6 characters
}
```
**Response:** `200 OK` or `400 Bad Request`

---

#### Login
```
POST /api/user/login
```
**Request Body:**
```json
{
  "login": "string",    // min 3 characters
  "password": "string"  // min 6 characters
}
```
**Response:**
```json
{
  "token": "string"
}
```

---

### Finance (Requires Authorization)

All finance endpoints require the `Authorization` header with the token received from login.

#### Get Balance
```
GET /api/finance/balance
```
**Response:**
```json
{
  "balance": 1000.00
}
```

---

#### Deposit
```
POST /api/finance/deposit
```
**Request Body:**
```json
{
  "amount": 100.00  // must be > 0
}
```
**Response:** `200 OK` or `400 Bad Request`

---

#### Transfer
```
POST /api/finance/transfer
```
**Request Body:**
```json
{
  "receiverLogin": "string",
  "amount": 50.00  // must be > 0
}
```
**Response:** `200 OK` or `400 Bad Request`

---

#### Transaction History
```
GET /api/finance/history
```
**Query Parameters:**
| Parameter | Type     | Description                    |
|-----------|----------|--------------------------------|
| from      | DateTime | (Optional) Start date filter   |
| to        | DateTime | (Optional) End date filter     |
| limit     | int      | Number of records (min 1)      |
| offset    | int      | Skip records (min 0)           |

**Response:**
```json
[
  {
    "senderName": "string",
    "receiverName": "string",
    "amount": 50.00,
    "date": "2025-01-27T12:00:00Z"
  }
]
```

---

## Database Structure

### Tables

#### users
| Column   | Type         | Constraints       |
|----------|--------------|-------------------|
| id       | int          | PK, Identity      |
| login    | nvarchar     | NOT NULL          |
| name     | nvarchar     | NOT NULL          |
| password | nvarchar     | NOT NULL          |

#### accounts
| Column   | Type          | Constraints                |
|----------|---------------|----------------------------|
| id       | int           | PK, Identity               |
| user_id  | int           | NOT NULL, FK -> users.id   |
| balance  | decimal(18,2) | NOT NULL                   |

#### sessions
| Column     | Type      | Constraints                |
|------------|-----------|----------------------------|
| user_id    | int       | PK, FK -> users.id         |
| token      | nvarchar  | NOT NULL                   |
| expires_at | datetime  | NOT NULL                   |

#### transactions
| Column              | Type          | Constraints                  |
|---------------------|---------------|------------------------------|
| id                  | int           | PK, Identity                 |
| sender_account_id   | int           | NOT NULL, FK -> accounts.id  |
| receiver_account_id | int           | NOT NULL, FK -> accounts.id  |
| created_at          | datetime      | NOT NULL                     |
| amount              | decimal(18,2) | NOT NULL                     |

