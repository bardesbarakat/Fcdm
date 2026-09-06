# CDM (Faculty Management System)

An ASP.NET MVC web application for managing faculty operations including study leaves, visiting professors, scientific missions, conferences, and secondments.

## Technologies Used
* **Framework:** ASP.NET MVC (.NET Framework 4.7.2)
* **ORM:** Entity Framework 6.5
* **Database:** SQL Server
* **Reporting:** Microsoft ReportViewer (.rdlc)

## Setup & Running Locally

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd cdm
   ```

2. **Database Configuration:**
   * Open `web.config`.
   * Update the `AppDbContext` connection string to point to your local SQL Server instance.

3. **Build & Run:**
   * Open the solution (`cdm.sln`) in Visual Studio.
   * Restore NuGet packages if they aren't restored automatically.
   * Build the project.
   * Run the application via IIS Express.

## Project Structure
* `Controllers/`: Contains the ASP.NET MVC controllers handling routing and business logic (Missions, Conferences, VisitingProfessors, etc.).
* `Models/`: Contains the data models and Entity Framework context.
* `Views/`: Contains the Razor view templates (`.cshtml`).
* `Reports/`: Contains RDLC files for generating PDF reports.
* `Content/` & `Scripts/`: Static assets including CSS stylesheets and JavaScript files.
