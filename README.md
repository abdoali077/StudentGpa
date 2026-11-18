# Student GPA Management System

A full-stack academic management system built with **ASP.NET Core Web API, Entity Framework Core, SQL Server, and Angular**.

The system manages students, academic courses, semesters, course enrollments, grading, and academic transcripts. It calculates Semester GPA and Cumulative GPA according to credit-hour weighting rules.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Features](#features)
- [Business Rules](#business-rules)
- [Technology Stack](#technology-stack)
- [System Architecture](#system-architecture)
- [Project Structure](#project-structure)
- [Database Design](#database-design)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Backend Setup](#backend-setup)
  - [Frontend Setup](#frontend-setup)
- [API Documentation](#api-documentation)
- [GPA Calculation](#gpa-calculation)
- [Sample Data](#sample-data)
- [Error Handling and Validation](#error-handling-and-validation)

---

## Project Overview

The Student GPA Management System provides an integrated solution for managing academic records and calculating student performance.

It allows users to:

- Manage student records.
- Manage academic courses and their credit hours.
- Create and manage academic semesters.
- Register students in courses.
- Record numerical grades or special academic statuses.
- Calculate Semester GPA and Cumulative GPA.
- View detailed academic transcripts.

The application follows a layered backend architecture to separate business rules, application logic, database operations, and API responsibilities.

---

## Features

### 1. Student Management

- Create new students.
- View student records.
- Update student information.
- Delete student records.
- Search students.
- Paginated student listing.
- Unique student codes.

### 2. Course Management

- Create and manage academic courses.
- Define course codes and names.
- Assign credit hours between 1 and 3.
- Update and delete courses.
- Search and paginate courses.
- Prevent duplicate course codes.

### 3. Semester Management

- Create academic semesters.
- Define semester codes and names.
- Set semester start and end dates.
- Update and delete semesters.
- Display available semesters.

### 4. Course Enrollment

- Register students in courses for specific semesters.
- Select students, courses, and semesters.
- Record numerical scores or special academic statuses.
- Update enrollment grades and statuses.
- Delete course registrations.
- Display student course registrations.

### 5. Grading System

- Database-driven grading scale.
- Automatic letter-grade assignment.
- Automatic grade-point calculation.
- Support for special academic statuses.

### 6. GPA and Academic Transcript

- Calculate Semester GPA.
- Calculate Cumulative GPA.
- Apply credit-hour weighting.
- Display registered and GPA-eligible hours.
- Display course grades and quality points.
- Display academic records grouped by semester.
- Display cumulative academic performance.

### 7. Dashboard

- Total number of students.
- Total number of courses.
- Total number of academic semesters.
- Quick navigation to the main application modules.

---

## Business Rules

The system implements the following academic rules:

### Course Credit Hours

- Each course must have between 1 and 3 credit hours.
- Credit hours are retrieved from the course record.
- Enrollment records preserve a snapshot of the course credit hours at registration time.

### Enrollment Rules

- A student cannot register for the same course twice in the same semester.
- The maximum registered credit hours per student per semester is 18.
- All registered courses count toward the 18-hour limit, including Withdrawn and Incomplete courses.
- Each enrollment must contain either a numerical score or a special academic status, not both.

### Grading Rules

Numerical scores must be between 0 and 100.

The grading scale is stored in the database and uses the following ranges:

| From | To | Grade | Points |
|---:|---:|:---:|---:|
| 96 | 101 | A+ | 4.0 |
| 92 | 96 | A | 3.7 |
| 88 | 92 | A- | 3.4 |
| 84 | 88 | B+ | 3.2 |
| 80 | 84 | B | 3.0 |
| 76 | 80 | B- | 2.8 |
| 72 | 76 | C+ | 2.6 |
| 68 | 72 | C | 2.4 |
| 64 | 68 | C- | 2.2 |
| 60 | 64 | D+ | 2.0 |
| 55 | 60 | D | 1.5 |
| 50 | 55 | D- | 1.0 |
| 0 | 50 | F | 0.0 |

Each range follows:

`FromGrade <= Score < ToGrade`

For example:

- 96 → A+
- 95.99 → A
- 50 → D-
- 49.99 → F

### Special Academic Statuses

| Status | Display Grade | Included in GPA? | Grade Points |
|---|---|---|---:|
| Absent | ABS | Yes | 0.0 |
| Withdrawn | W | No | N/A |
| Incomplete | I | No | N/A |

**Important:**

- ABS contributes zero quality points, but its credit hours are included in GPA calculations.
- W and I are excluded from both GPA numerator and denominator.
- W and I still count toward the semester registration limit.

---

## Technology Stack

### Backend

- C#
- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- Microsoft SQL Server
- FluentValidation
- Swagger / OpenAPI

### Frontend

- Angular 21
- TypeScript
- HTML5
- SCSS
- Bootstrap
- Bootstrap Icons
- Angular Router
- Angular HttpClient

### Architecture and Development Practices

- Layered Architecture
- Dependency Injection
- Repository Pattern
- Unit of Work Pattern
- Domain-driven business rules
- DTOs
- Asynchronous programming
- CancellationToken
- Database migrations

---

## System Architecture

The backend is organized into four main layers.

### Domain Layer

Contains the core business model:

- Domain entities.
- Domain interfaces.
- Domain services.
- Business policies.
- Domain errors.
- Shared domain abstractions.

### Application Layer

Responsible for application use cases:

- Application services.
- DTOs.
- Service interfaces.
- Validation.
- Application-level business workflows.

### Infrastructure Layer

Responsible for external and persistence concerns:

- Entity Framework Core.
- SQL Server database configuration.
- DbContext.
- Entity configurations.
- Repository implementations.
- Unit of Work implementation.
- Database migrations.
- Seed data.

### API Layer

Provides HTTP access to application functionality:

- API controllers.
- Dependency injection configuration.
- Middleware and application configuration.
- CORS configuration.
- Swagger documentation.
- HTTP response handling.

---

## Project Structure

```text
StudentGpa/
│
├── src/
│   ├── StudentGpa.Domain/
│   │
│   ├── StudentGpa.Application/
│   │
│   ├── StudentGpa.Infrastructure/
│   │
│   └── StudentGpa.API/
│
├── frontend/
│   └── student-gpa/
│       ├── src/
│       │   └── app/
│       │       ├── core/
│       │       │   ├── config/
│       │       │   ├── models/
│       │       │   └── services/
│       │       │
│       │       ├── features/
│       │       │   ├── dashboard/
│       │       │   ├── students/
│       │       │   ├── courses/
│       │       │   ├── semesters/
│       │       │   ├── enrollments/
│       │       │   └── transcript/
│       │       │
│       │       └── layout/
│       │
│       └── package.json/
│
└── README.md
```

---

## Database Design

The application uses Microsoft SQL Server with Entity Framework Core.

The main entities include:

- **Student:** Stores student information.
- **Course:** Stores course information and credit hours.
- **Semester:** Stores academic semester information.
- **Enrollment:** Connects students with courses and semesters, storing scores, statuses, and calculated grade information.
- **GradeScale:** Stores the grading ranges and corresponding grade points.

The Enrollment entity represents a student's registration in a specific course during a specific semester.

---

## Getting Started

### Prerequisites

Install the following tools:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/)
- [Angular CLI](https://angular.dev/tools/cli)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server)
- [SQL Server Management Studio](https://learn.microsoft.com/en-us/ssms/install/install)

### Backend Setup

**1. Clone the repository**

```bash
git clone <repository-url>
cd StudentGpa
```

**2. Configure the database connection**

Open:

`src/StudentGpa.API/appsettings.json`

Configure the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.;Initial Catalog=StudentGpaDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

Update the connection string according to your SQL Server configuration.

**3. Restore dependencies**

```bash
dotnet restore
```

**4. Apply Entity Framework Core migrations**

```bash
dotnet ef database update --project src/StudentGpa.Infrastructure --startup-project src/StudentGpa.API
```

This creates or updates the database using the available migrations.

**5. Run the API**

```bash
dotnet run --project src/StudentGpa.API
```

The API will be available at the configured application URL.

Swagger documentation:

`http://localhost:5144/swagger/index.html`

If the application uses a different port, use the URL displayed in the terminal.

### Frontend Setup

**1. Navigate to the Angular project**

```bash
cd frontend/student-gpa
```

**2. Install dependencies**

```bash
npm install
```

**3. Configure the API URL**

Open:

`src/app/core/config/api.config.ts`

```typescript
export const API_CONFIG = {
  baseUrl: 'http://localhost:5144/api'
};
```

Make sure the URL matches the running backend API.

**4. Start the Angular application**

```bash
npx ng serve
```

Open:

`http://localhost:4200`

---

## API Documentation

The backend exposes RESTful API endpoints for managing academic data.

Base URL:

`http://localhost:5144/api`

### Students

| Method | Endpoint | Description |
|---|---|---|
| GET | `/students` | Get paginated students |
| GET | `/students/{id}` | Get student by ID |
| POST | `/students` | Create a student |
| PUT | `/students/{id}` | Update a student |
| DELETE | `/students/{id}` | Delete a student |

Student listing supports pagination and search:

`GET /api/students?page=1&pageSize=20&search=Ahmed`

### Courses

| Method | Endpoint | Description |
|---|---|---|
| GET | `/courses` | Get paginated courses |
| GET | `/courses/{id}` | Get course by ID |
| POST | `/courses` | Create a course |
| PUT | `/courses/{id}` | Update a course |
| DELETE | `/courses/{id}` | Delete a course |

### Semesters

| Method | Endpoint | Description |
|---|---|---|
| GET | `/semesters` | Get all semesters |
| GET | `/semesters/{id}` | Get semester by ID |
| POST | `/semesters` | Create a semester |
| PUT | `/semesters/{id}` | Update a semester |
| DELETE | `/semesters/{id}` | Delete a semester |

### Enrollments

| Method | Endpoint | Description |
|---|---|---|
| GET | `/enrollments/students/{studentId}` | Get a student's enrollments |
| POST | `/enrollments` | Register a student in a course |
| PUT | `/enrollments/{enrollmentId}/grade` | Update enrollment grade/status |
| DELETE | `/enrollments/{enrollmentId}` | Delete an enrollment |

### GPA and Transcript

| Method | Endpoint | Description |
|---|---|---|
| GET | `/gpa/students/{studentId}/transcript` | Get the student's academic transcript and GPA results |

---

## GPA Calculation

The system uses a credit-hour weighted GPA calculation.

### Course Quality Points

```text
Course Quality Points = Grade Points × Credit Hours
```

Example:

A student gets 3.4 grade points in a 3-credit-hour course:

```text
3.4 × 3 = 10.2 Quality Points
```

### Semester GPA

```text
Semester GPA =
    Total Eligible Quality Points
    -----------------------------
       Total Eligible Credit Hours
```

### Cumulative GPA

```text
Cumulative GPA =
    Total Quality Points Across All Semesters
    ------------------------------------------
      Total Eligible Credit Hours Across All Semesters
```

The Cumulative GPA is weighted across all eligible courses, not calculated as the arithmetic average of semester GPAs.

GPA values are displayed to two decimal places while retaining calculation precision internally.

If a student has no eligible credit hours, the GPA should be displayed as `N/A`.

---

## Sample Data

The development database includes sample academic records for testing the system, including:

- Multiple students.
- Multiple academic courses.
- Multiple semesters.
- Course enrollments.
- Different numerical grades.
- Special academic statuses such as ABS, W, and I.

These records can be used to verify enrollment rules, grading behavior, and GPA calculations.

---

## Error Handling and Validation

The backend applies validation and business rules before persisting academic operations.

Examples include:

- Invalid student, course, or semester IDs.
- Duplicate student or course codes.
- Duplicate course enrollment.
- Invalid credit hours.
- Invalid numerical scores.
- Semester credit-hour limit violations.
- Invalid combinations of numerical scores and special statuses.
- Missing related entities.

The API uses appropriate HTTP response codes, including:

- `200 OK`
- `201 Created`
- `204 No Content`
- `400 Bad Request`
- `404 Not Found`
- `409 Conflict`
- `422 Unprocessable Entity`

---

## License

This project was developed as a technical assessment for a Student GPA Management System.
