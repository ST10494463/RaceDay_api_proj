# RaceDAY API – Part 2

## Project Overview

RaceDAY is an ASP.NET Core Web API for managing running, walking, and cycling events. This project was developed for Programming Assignment Part 2 and uses Entity Framework Core with a Code-First approach to work with a SQL Server database.

The API supports user registration and login, role-based access, event management, categories, event elements, enrolments, and results.

## Technologies Used

- C#
- ASP.NET Core Web API
- .NET 8
- Entity Framework Core (Code-First)
- SQL Server / SQL Server LocalDB
- JWT Bearer authentication
- Swagger / OpenAPI
- xUnit for automated tests
- GitHub Actions (CI/CD setup required)

## Main Features

### Authentication and Roles

- Users can register as either an **Organiser** or a **Participant**.
- Users can log in with their email address and password.
- Passwords are hashed before being stored.
- JWT bearer tokens are used to authenticate subsequent API requests.
- Role-based authorization restricts endpoints to the appropriate role.

### User Profiles

- Participants can view and update their own profile.
- Profile responses do not expose the stored password hash.

### Events

- Organisers can create, update, and delete their own events.
- Events can be viewed publicly.
- Event details include the name, description, date, location, distance, event type, and status.
- Event types are intended to represent **Run**, **Walk**, or **Cycle**.

### Categories and Event Elements

- Organisers can manage categories for events.
- Categories can include age limits and an entry fee.
- Organisers can manage event elements associated with an event and category.
- Both types of information can be retrieved through API endpoints.

### Enrolments

- Participants can enrol in events by selecting a category.
- Enrolments link the participant, event, category, and optional event element.
- Participants can view their own enrolments and cancel their enrolments.
- Organisers can view enrolments for events they own and update enrolment status.

### Results

- Organisers can record and update participant results.
- Results can include finishing position and finish time.
- Participants can view their own results.

## Database Entities

The API uses the following main entities:

- `Organizer`
- `Participant`
- `Event`
- `Category`
- `EventElement`
- `Enrolment`
- `Result`

The database context is `ApplicationDBContext`.

## Getting Started

### Prerequisites

- Visual Studio with the .NET 8 SDK
- SQL Server or SQL Server LocalDB
- NuGet package restore enabled

### 1. Clone the repository

```bash
git clone <your-repository-url>
cd RaceDay_api_proj
```

Replace `<your-repository-url>` with your GitHub repository URL.

### 2. Configure the database connection

Open `appsettings.json` and check the `DefaultConnection` setting. For example, a LocalDB connection may look like:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\api_both;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Use the SQL Server instance and database name configured on your own machine.

### 3. Configure JWT settings

The project uses JWT settings named `Key`, `Issuer`, `Audience`, and `ExpiryMinutes` in the `Jwt` section of configuration.

For local development, configure these values using .NET User Secrets or environment variables where possible. Do not commit real signing keys or production secrets to GitHub. The development key in a local configuration file should be replaced with a secure secret before deployment.

### 4. Restore packages and build

Open the solution in Visual Studio, then select **Build → Build Solution**.

Alternatively, from the directory containing the API project file, run:

```bash
dotnet restore
dotnet build
```

### 5. Apply database migrations

In Visual Studio, open **Tools → NuGet Package Manager → Package Manager Console** and ensure the API project is selected as the default project. Run:

```powershell
Update-Database
```

If using the .NET CLI, run the migration command from the directory containing the API `.csproj` file:

```bash
dotnet ef database update
```

If the Entity Framework CLI tool is not installed, install it according to the official .NET documentation.

### 6. Run the API

Run `RaceDay_api_proj` from Visual Studio. Open the Swagger URL shown in the browser or application output to explore and test the endpoints.

## API Endpoint Overview

The following routes are implemented in the project as currently developed. Use Swagger for the exact request body schemas and response details.

| Area | Method | Route | Access |
|---|---|---|---|
| Authentication | POST | `/api/Auth/register` | Public |
| Authentication | POST | `/api/Auth/login` | Public |
| Events | GET | `/api/Events` | Public |
| Events | POST | `/api/Events` | Organiser |
| Events | PUT | `/api/Events/{id}` | Organiser |
| Events | DELETE | `/api/Events/{id}` | Organiser |
| Categories | GET | `/api/Categories/event/{eventId}` | Public |
| Categories | POST | `/api/Categories` | Organiser |
| Categories | PUT | `/api/Categories/{id}` | Organiser |
| Categories | DELETE | `/api/Categories/{id}` | Organiser |
| Event Elements | GET | `/api/EventElements/category/{categoryId}` | Public |
| Event Elements | POST | `/api/EventElements` | Organiser |
| Event Elements | PUT | `/api/EventElements/{id}` | Organiser |
| Event Elements | DELETE | `/api/EventElements/{id}` | Organiser |
| Enrolments | GET | `/api/Enrolments` | Authenticated; results filtered by role |
| Enrolments | POST | `/api/Enrolments` | Participant |
| Enrolments | PUT | `/api/Enrolments/{id}/cancel` | Participant |
| Enrolments | PUT | `/api/Enrolments/{id}/status` | Organiser |
| Results | GET | `/api/Results` | Authenticated; results filtered by role |
| Results | POST | `/api/Results` | Organiser |
| Results | PUT | `/api/Results/{id}` | Organiser |
| Participant Profile | GET | `/api/Participants/profile` | Participant |
| Participant Profile | PUT | `/api/Participants/profile` | Participant |

Authenticated endpoints require the JWT bearer token returned by the login endpoint. In Swagger, use **Authorize** and enter the token as required by the Swagger configuration.

## Running Automated Tests

The test project is named `RaceDay_api_proj.Tests` and uses xUnit. It includes a `RaceDayApiFactory` test host and an in-memory Entity Framework Core database for API tests.

To run tests in Visual Studio:

1. Select **Test → Test Explorer**.
2. Click **Run All Tests**.
3. Review passed and failed tests.

To run tests from the command line, run this from the solution directory:

```bash
dotnet test
```

**Testing status:** At the time this README was drafted, the test suite had 8 tests, with 5 passing and 3 failing after the test database setup was changed. Resolve those failures and confirm that all tests pass before submitting the assignment.

## GitHub and CI/CD

The assignment requires a GitHub Actions workflow that builds the API and runs automated tests on every push.

Before submission:

- Push the Part 2 work to the required GitHub repository and clearly named branch or subfolder.
- Ensure the repository contains at least 20 meaningful commits made through your own GitHub account.
- Add a workflow under `.github/workflows/` that restores dependencies, builds the solution, and runs tests.
- Confirm the workflow finishes successfully.
- Add a screenshot of the successful green GitHub Actions build to this README or the repository documentation.

## Security Notes

- Never store plain-text passwords.
- Keep JWT signing keys and other secrets out of source control.
- Use HTTPS for deployed environments.
- Verify authorization rules when adding or changing endpoints.

## Assignment Checklist

- [x] ASP.NET Core Web API project created using .NET 8.
- [x] EF Core Code-First and SQL Server configured.
- [x] Registration and login endpoints implemented.
- [x] Password hashing and JWT authentication implemented.
- [x] Role-based authorization added.
- [x] Event, category, event element, enrolment, and result controllers created.
- [x] Swagger configured.
- [ ] Confirm every endpoint has a clear Swagger description.
- [ ] Fix failing automated tests and confirm all tests pass.
- [ ] Complete and verify GitHub Actions CI/CD.
- [ ] Confirm 20 meaningful commits.
- [ ] Add screenshot of a successful green CI build.
