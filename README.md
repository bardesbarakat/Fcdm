<h1 align="center">
  <br>
  Fcdm - Faculty Management System
  <br>
</h1>

<p align="center">
  <a href="https://dotnet.microsoft.com/en-us/apps/aspnet/mvc">
    <img src="https://img.shields.io/badge/ASP.NET_MVC-%235C2D91.svg?style=for-the-badge&logo=.net&logoColor=white" alt="ASP.NET MVC" />
  </a>
  <a href="https://learn.microsoft.com/en-us/ef/">
    <img src="https://img.shields.io/badge/Entity_Framework-%23512BD4.svg?style=for-the-badge&logo=.net&logoColor=white" alt="Entity Framework" />
  </a>
  <a href="https://www.microsoft.com/en-us/sql-server">
    <img src="https://img.shields.io/badge/SQL_Server-CC292B?style=for-the-badge&logo=microsoft-sql-server&logoColor=white" alt="SQL Server" />
  </a>
  <a href="https://getbootstrap.com/">
    <img src="https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap" />
  </a>
  <a href="https://jquery.com/">
    <img src="https://img.shields.io/badge/jQuery-0769AD?style=for-the-badge&logo=jquery&logoColor=white" alt="jQuery" />
  </a>
</p>

<p align="center">
  <strong>A comprehensive digital solution for universities to manage faculty affairs, missions, and study leaves.</strong>
</p>

## 📝 Project Overview

**Fcdm** is a full-stack ASP.NET MVC web application designed to streamline and digitize administrative workflows within higher education institutions. It replaces manual paperwork with a centralized dashboard that efficiently tracks faculty personnel, study leaves, scientific missions, university conferences, and secondments. By automating these processes, the system drastically reduces administrative overhead and ensures accurate, real-time data tracking for university management.

## ✨ Key Features

- 👤 **Faculty Member Management:** Comprehensive CRUD operations to seamlessly manage and track faculty personnel data and status.
- ✈️ **Missions & Secondments Tracking:** Automated workflows for recording and monitoring internal/external scientific missions and faculty secondments.
- 📚 **Study Leaves:** End-to-end management of study leave applications, including uploading executive decisions and university president approvals.
- 📊 **Dynamic PDF Reporting:** Built-in comprehensive analytics and printable reports powered by Microsoft ReportViewer (.rdlc).
- 🔒 **Secure Authentication:** Robust Forms-based authentication to ensure sensitive faculty data is only accessible to authorized administrators.
- 🗂️ **Document Management:** Integrated file management for securely uploading, storing, and viewing official PDF documents and approvals.

## 🛠️ Tech Stack

- **Backend:** C#, ASP.NET MVC (.NET Framework 4.7.2)
- **Database / ORM:** Microsoft SQL Server, Entity Framework 6.5
- **Frontend:** HTML5, CSS3, JavaScript, jQuery, Bootstrap
- **Reporting:** Microsoft RDLC Report Designer

## 📸 Screenshots

<!-- Add your screenshots here -->
> *Placeholder for application screenshots (Dashboard, Reports, Data Entry Forms)*
> 
> ![Dashboard](https://via.placeholder.com/800x400.png?text=Dashboard+Screenshot)
> ![Reports](https://via.placeholder.com/800x400.png?text=Reports+Screenshot)

## 📁 Project Structure

```text
Fcdm/
├── Controllers/       # ASP.NET MVC Controllers (Missions, Conferences, Account, etc.)
├── Models/            # Entity Framework Context, DbSets, and Data Models
├── Views/             # Razor View Engine templates (.cshtml)
├── Reports/           # Microsoft ReportViewer files (.rdlc) for dynamic PDF generation
├── Content/           # Static CSS files and Bootstrap themes
├── Scripts/           # Static JavaScript and jQuery files
├── uploads/           # Secure directory for uploaded official PDF documents
└── web.config         # Application and database configuration
```

## 🚀 Getting Started

Follow these steps to set up and run the project locally on your machine.

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with ASP.NET and web development workload)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or any SQL Server instance)
- .NET Framework 4.7.2 SDK

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/bardesbarakat/Fcdm.git
   cd Fcdm
   ```

2. **Configure the Database:**
   - Open the `web.config` file in the root directory.
   - Locate the `<connectionStrings>` section.
   - Update the `AppDbContext` and `cdmConnectionString` data source values to point to your local SQL Server instance (e.g., `.\SQLEXPRESS`).

3. **Build the Solution:**
   - Open `cdm.sln` in Visual Studio.
   - Visual Studio should automatically restore the required NuGet packages. If not, right-click the solution and select **Restore NuGet Packages**.
   - Press `Ctrl + Shift + B` to build the project.

4. **Run the Application:**
   - Press `F5` or click **Start** in Visual Studio to launch the application using IIS Express.

## 👨‍💻 Author & Contact

**Bardes Barakat**

- GitHub: [@bardesbarakat](https://github.com/bardesbarakat)
- LinkedIn: [Your LinkedIn Profile](https://www.linkedin.com/in/your-profile) *(Update this link)*
- Email: your.email@example.com *(Update this email)*

---
⭐️ *If you find this project interesting or helpful, please consider giving it a star!*
