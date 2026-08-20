# Product Management Platform

A production-ready Product Management solution built with **ASP.NET Core 8 Web API** and modern **React (Vite)** frontend. The solution implements Clean Architecture, JWT Authentication, EF Core with SQLite, Search & Price Range Filtering, Serilog Structured Logging, Global Exception Handling, and a comprehensive xUnit test suite.

---

## 🏛️ Architecture & Tech Stack

- **Backend:** ASP.NET Core 8 Web API (Clean / Layered Architecture)
  - `ProductManagement.Core`: Domain Entities, DTOs with Data Annotations, Repository & Service Contracts, Custom Exceptions.
  - `ProductManagement.Infrastructure`: EF Core DbContext, SQLite Data Layer, BCrypt Hashing, JWT Token Service, Repositories, Database Initializer & Seeder.
  - `ProductManagement.API`: REST Controllers, JWT Bearer Authentication & Authorization, Swagger OpenAPI with JWT Bearer support, Serilog Request Logger, Global Exception Middleware.
  - `ProductManagement.Tests`: Automated Unit Tests and API Integration Tests with `xUnit`, `Moq`, `FluentAssertions`, and `WebApplicationFactory`.
- **Database:** SQLite (Embedded, cross-platform, zero-configuration on Windows and *NIX/Linux).
- **Frontend:** React 19 + Vite (Modern, responsive, token-based authentication, product search & filter, modal forms, toast alerts).
- **Logging:** Serilog (Structured console sink + daily rolling file sink in `logs/`).
- **Containerization:** Docker & Docker Compose.

---

## 🚀 How to Run Locally (5 Simple Steps)

Follow these steps to run the application on your local machine:

### **Step 1: Clone the Repository**
```bash
git clone https://github.com/rostedpotato/technical-test-net.git
cd technical-test-net
```

### **Step 2: Build the Solution**
```bash
dotnet build
```

### **Step 3: Run the Backend API**
```bash
dotnet user-secrets set "JwtSettings:Secret" "replace-this-with-a-random-secret-at-least-32-bytes" --project src/ProductManagement.API
dotnet run --project src/ProductManagement.API
```
> The API will start at **`http://localhost:5187`** and automatically initialize SQLite database migrations and sample data seeding.
> The JWT secret is intentionally stored in user-secrets locally and must be supplied through `JwtSettings__Secret` in deployed environments.

### **Step 4: Run the Frontend Client**
Open a new terminal tab and run:
```bash
cd client
npm install
npm run dev
```

### **Step 5: Open in Your Browser**
- **Frontend Web App:** [http://localhost:5173](http://localhost:5173)
- **Swagger API Documentation:** [http://localhost:5187/swagger](http://localhost:5187/swagger)

---

## 🐳 Alternative: Run with Docker

If you have Docker installed, you can run both the Backend and Frontend with a single command:

```bash
export JWT_SECRET="replace-this-with-a-random-secret-at-least-32-bytes"
docker compose up --build
```
On PowerShell, use `$env:JWT_SECRET = "replace-this-with-a-random-secret-at-least-32-bytes"` before running Docker Compose.
For cloud deployment, also set `VITE_API_URL` to the public API URL and `FRONTEND_ORIGIN` to the public frontend origin.
- **Web Client:** `http://localhost:3000`
- **Backend API & Swagger:** `http://localhost:5187/swagger`

---

## 🔐 Default Demo Accounts

The database is pre-seeded with the following accounts for immediate testing:

| Username | Password | Role | Description |
| :--- | :--- | :---: | :--- |
| `admin` | `Admin123!` | **Admin** | Full access to create, edit, delete, and view products. |
| `demo_user` | `User123!` | **User** | Can view and search products. |

*You can also register new accounts directly from the UI or via `/api/auth/register`.*

---

## 📡 API Endpoints Summary

### Authentication Endpoints
- `POST /api/auth/register` - Register a new user account.
- `POST /api/auth/login` - Authenticate user and receive signed JWT token.
- `GET /api/auth/me` - Get profile of current authenticated user (`[Authorize]`).

### Product Management Endpoints
- `GET /api/products` - Get paginated products with optional search and filters (`[Authorize]`):
  - `keyword` (string): Search in product Name and Description.
  - `minPrice` (decimal): Filter by minimum price.
  - `maxPrice` (decimal): Filter by maximum price.
  - `page` (int, default: 1) & `pageSize` (int, default: 10).
  - `sortBy` (string: `CreatedAt`, `Price`, `Name`) & `sortDescending` (bool).
- `GET /api/products/{id}` - Get product details by ID (`[Authorize]`).
- `POST /api/products` - Create new product (`[Authorize(Roles = "Admin")]`, Data Annotations validation).
- `PUT /api/products/{id}` - Update product by ID (`[Authorize(Roles = "Admin")]`, Data Annotations validation).
- `DELETE /api/products/{id}` - Delete product by ID (`[Authorize(Roles = "Admin")]`).

---

## 🧪 Automated Testing

Execute all automated unit and integration tests with:

```bash
dotnet test
```

**Test Results: 20 Passed / 0 Failed (100% Pass Rate)**
- `ProductServiceTests`: CRUD service behavior and response mapping.
- `AuthServiceTests`: Duplicate username/email checks, BCrypt password hashing, JWT generation, login security.
- `ProductApiIntegrationTests`: HTTP endpoint tests with `WebApplicationFactory`, authorization, authenticated CRUD, search, price filtering, and invalid-range validation.

---

## 💡 Assumptions & Technical Decisions

1. **Database Selection (SQLite):** SQLite was chosen for zero-configuration portability across Windows, macOS, and Linux environments without requiring an external database server.
2. **Password Security:** Password hashing uses **BCrypt** with salted hashes.
3. **Data Validation:** Input models utilize **Data Annotations** (`[Required]`, `[StringLength]`, `[Range]`) with descriptive error messages.
4. **Structured Logging:** **Serilog** outputs formatted logs to both standard output and daily rolling log files in `logs/` for production traceability.
5. **Global Exception Handling:** Custom middleware ensures no unhandled exceptions leak stack traces in production, returning uniform `ApiResponse<T>` JSON envelopes.
6. **Configuration:** JWT secrets are supplied through .NET user-secrets locally and environment variables in Docker/cloud deployments. CORS is restricted to configured frontend origins.
