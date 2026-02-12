Hints Document 
1. The database schema provided below is designed without explicit foreign key 
relationships between tables. You may use it as-is, modify it, or introduce 
relationships based on your design preferences. 
This approach allows flexibility in defining table relations, indexing strategies, or 
constraints according to project requirements. 
You are encouraged to analyze the schema and determine the most suitable 
relationships, ensuring the system remains scalable, maintainable, and aligned with 
best practices. 
1. Roles Table 
CREATE TABLE Roles ( 
RoleId INT IDENTITY PRIMARY KEY, 
Name NVARCHAR(50) NOT NULL UNIQUE, 
Description NVARCHAR(250) NULL 
); 
2. Users Table 
CREATE TABLE Users ( 
UserId INT IDENTITY PRIMARY KEY, 
UserName NVARCHAR(50) NOT NULL UNIQUE, 
Email NVARCHAR(100) NOT NULL UNIQUE, 
PasswordHash NVARCHAR(500) NOT NULL, 
RoleId INT NOT NULL,   hint: relates to Roles 
RefreshToken NVARCHAR(500) NULL, 
RefreshTokenExpiry DATETIME NULL, 
CreatedAt DATETIME NOT NULL DEFAULT GETDATE() 
); 
3. Servers Table 
CREATE TABLE Servers ( 
ServerId INT IDENTITY PRIMARY KEY, 
Name NVARCHAR(100) NOT NULL, 
IPAddress NVARCHAR(50) NULL, 
Status NVARCHAR(20) NOT NULL DEFAULT 'Up', 
Description NVARCHAR(250) NULL, 
CreatedAt DATETIME NOT NULL DEFAULT GETDATE() 
); 
4. Metrics Table 
CREATE TABLE Metrics ( 
MetricId INT IDENTITY PRIMARY KEY, 
ServerId INT NOT NULL,   hint: relates to Servers 
CpuUsage FLOAT NOT NULL, 
MemoryUsage FLOAT NOT NULL, 
DiskUsage FLOAT NOT NULL, 
ResponseTime FLOAT NOT NULL, 
Status NVARCHAR(20) NOT NULL, 
Timestamp DATETIME NOT NULL DEFAULT GETDATE() 
); 
5. Disks Table 
CREATE TABLE Disks ( 
DiskId INT IDENTITY PRIMARY KEY, 
ServerId INT NOT NULL,   hint: relates to Servers 
DriveLetter NVARCHAR(5) NOT NULL, 
FreeSpaceMB BIGINT NOT NULL, 
TotalSpaceMB BIGINT NOT NULL, 
UsedPercentage FLOAT NOT NULL, 
Timestamp DATETIME NOT NULL DEFAULT GETDATE() 
); 
6. Alerts Table 
CREATE TABLE Alerts ( 
    AlertId INT IDENTITY PRIMARY KEY, 
    ServerId INT NOT NULL,   hint: relates to Servers 
    MetricType NVARCHAR(50) NOT NULL, 
    MetricValue FLOAT NOT NULL, 
    Threshold FLOAT NOT NULL, 
    Status NVARCHAR(20) NOT NULL DEFAULT 'Triggered', 
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(), 
    ResolvedAt DATETIME NULL 
); 
 
  7. Reports Table 
CREATE TABLE Reports ( 
    ReportId INT IDENTITY PRIMARY KEY, 
    ServerId INT NOT NULL,   hint: relates to Servers 
    StartTime DATETIME NOT NULL, 
    EndTime DATETIME NOT NULL, 
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', 
    FilePath NVARCHAR(500) NULL, 
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(), 
    CompletedAt DATETIME NULL 
); 
 
 
 
 
 
 
 
 
 
 
 
 
2. Application Hints:- 
you can use this library  
Namespace: System.Diagnostics 
Class: PerformanceCounter 
Purpose: Allows you to read Windows performance metrics like CPU, memory, 
disk, network, etc. 
Sample: 
public class SystemMetricsService 
{ 
private PerformanceCounter _cpuCounter; 
private PerformanceCounter _ramCounter; 
private PerformanceCounter _diskCounter; 
public SystemMetricsService() 
{ 
// Initialize performance counters 
_cpuCounter = new PerformanceCounter("Processor", "% Processor Time", 
"_Total"); 
_ramCounter = new PerformanceCounter("Memory", "% Committed Bytes 
In Use"); 
_diskCounter = new PerformanceCounter("LogicalDisk", "% Free Space", 
"_Total"); 
} 
} 



Senior Full Stack Developer - Technical Assessment 
Senior Full Stack Developer - Technical 
Assessment 
Overview 
This technical assessment is designed to evaluate your skills in building production-ready, scalable 
applications using .NET Core and modern frontend frameworks. You will create a Real-Time 
Monitoring Dashboard that demonstrates your expertise in clean architecture, SOLID principles, 
background job processing, real-time communications, and DevOps practices. 
Submission: GitHub repository with complete source code, documentation, and deployment 
guidelines 
Project: Real-Time System Monitoring Dashboard 
Build a comprehensive system monitoring dashboard that tracks and visualizes system metrics, 
processes background analysis jobs, and provides real-time updates to connected clients. 
Business Context 
The application simulates a monitoring system for multiple servers/services. It should collect metrics 
(CPU, memory, disk usage, response times, etc.), process this data in the background, detect 
anomalies, generate reports, and provide real-time updates to dashboard users. 
Technical Requirements 
Backend (.NET Core) 
1. Framework & Architecture 
• .NET 10.0 
• Clean Architecture implementation with clear separation of layers: 
1. Domain Layer (Entities, Domain Events, Domain Services) 
2. Application Layer (Use Cases, Interfaces, DTOs, Validators) 
3. Infrastructure Layer (Data Access, External Services, File System) 
4. Presentation Layer (API Controllers, SignalR Hubs) 
• SOLID Principles demonstrated throughout the codebase 
• Dependency Injection properly configured 
1 
Senior Full Stack Developer - Technical Assessment 
• MediatR or similar pattern for CQRS implementation (optional but recommended) 
2. Entity Framework Core 
• Database of your choice (SQL Server, PostgreSQL, MySQL, SQLite) preferred SQL 
• Database-First approach 
• Proper entity configurations and relationships 
• Repository pattern or direct EF Core usage (justify your choice) 
• Implement at least 
• One-to-Many relationships (e.g., Server to Metrics) 
• Many-to-Many relationships (e.g., Users to Roles/Permissions) 
• Complex queries with filtering, sorting, and pagination 
• Seed data for testing and demonstration 
• Dapper (Optional) 
3. Hangfire Background Jobs 
Implement the following background processing scenarios: 
• Recurring Job: Collect system metrics every 1-5 minutes (simulate with random data) 
• Fire-and-Forget Job: Generate reports on-demand 
• Delayed Job: Schedule maintenance tasks or alerts 
• Continuation Job: Process data after report generation 
• Proper job persistence and monitoring 
• Dashboard for viewing job status (Hangfire built-in dashboard) 
4. Real-Time Functionality (SignalR) 
• Real-time metric updates pushed to connected clients 
• Alert notifications for threshold breaches 
• Live user presence indicators 
• Proper connection management and hub architecture 
5. RESTful API 
• Well-structured REST endpoints following conventions 
• Proper HTTP status codes and error handling 
• API versioning (at least demonstrate the approach) 
• Input validation using FluentValidation or Data Annotations 
• Global exception handling middleware 
• Request/Response logging 
6. Authentication & Authorization 
• JWT-based authentication 
• Role-based authorization (Admin, User roles minimum) 
• Secure password hashing 
2 
Senior Full Stack Developer - Technical Assessment 
• Refresh token implementation (optional but recommended) 
7. Additional Backend Features 
• Health checks endpoint 
• Swagger/OpenAPI documentation 
• Serilog or similar structured logging 
• Configuration management (appsettings, environment variables) 
• Rate limiting (optional) 
Frontend (Angular OR React with TypeScript) 
Choose ONE framework based on your expertise. 
Core Requirements (Both Frameworks) 
1. TypeScript throughout the application 
Component Architecture - Reusable, modular components - Smart vs. Presentational component pattern - Proper state management (NgRx/Redux/Context API/Zustand) 
2. Real-Time Dashboard Features - Live metric charts (CPU, Memory, Disk, Network) - Real-time updates via SignalR connection - Alert notifications display - Server/service list with health status - Historical data visualization 
3. Key Pages/Views - Login/Authentication page - Dashboard overview (main monitoring page) - Server details page with metrics history - Reports page (generate and view reports) - Background jobs monitoring page - User management (Admin only) 
4. UI/UX Requirements - Responsive design (mobile, tablet, desktop) 
3 
Senior Full Stack Developer - Technical Assessment - Modern UI framework (Angular Material, Material-UI, Ant Design, Tailwind CSS, etc.) - Loading states and error handling - Form validation with clear error messages - Dark/Light theme (optional but impressive) 
5. Charting Library - Chart.js, Recharts, ApexCharts, or similar - Real-time updating charts - Interactive and responsive visualizations 
6. HTTP Communication - HTTP interceptors for auth tokens and error handling - Proper async/await or Observable patterns - API service layer abstraction 
Angular-Specific (If Chosen) 
• Angular 17+ with standalone components (recommended) or modules 
• RxJS for reactive programming 
• Angular Guards for route protection 
• Lazy loading for routes 
• NgRx or Akita for state management (for complex state) 
React-Specific (If Chosen) 
• React 18+ with functional components and hooks 
• React Router for navigation 
• Protected routes implementation 
• Custom hooks for reusable logic 
• Context API with useReducer OR Redux Toolkit/Zustand for state management 
• React Query or SWR for server state (optional) 
Architecture & Design Expectations 
Clean Architecture Layers 
Your project should follow this structure: 
src/ 
4 
Senior Full Stack Developer - Technical Assessment 
 5 
  Domain/ 
    Entities/ 
    Enums/ 
    Events/ 
    Exceptions/ 
    Interfaces/ 
  Application/ 
    Common/ 
      Interfaces/ 
      Models/ 
      Behaviors/ 
    Features/ 
      Metrics/ 
      Servers/ 
      Reports/ 
      Users/ 
    DependencyInjection.cs 
  Infrastructure/ 
    Persistence/ 
      Configurations/ 
      Migrations/ 
      ApplicationDbContext.cs 
    BackgroundJobs/ 
    Services/ 
    DependencyInjection.cs 
  WebAPI/ 
    Controllers/ 
    Hubs/ 
    Middleware/ 
    Filters/ 
    Program.cs 
 
SOLID Principles Demonstration 
Ensure your code demonstrates: 
• Single Responsibility: Each class has one clear purpose 
• Open/Closed: Extensible without modification (use interfaces, abstractions) 
Senior Full Stack Developer - Technical Assessment 
• Liskov Substitution: Derived classes properly substitute base classes 
• Interface Segregation: Focused interfaces, not fat interfaces 
• Dependency Inversion: Depend on abstractions, not concretions 
Testing Requirements 
Backend Tests 
Unit Tests 
• Application layer (use cases/handlers) 
• Domain logic and validations 
• At least 60% code coverage for critical paths 
• Use xUnit, NUnit, or MSTest 
• Moq or NSubstitute for mocking 
Integration Tests 
• API endpoints testing 
• Database operations with in-memory or test database 
• WebApplicationFactory for integration tests 
• At least 3-5 key integration test scenarios 
Frontend Tests 
Unit Tests 
• Services/API layer 
• Utilities and helper functions 
• Redux reducers or state management logic (if applicable) 
• Jest + Testing Library (React) or Jasmine/Karma (Angular) 
Component Tests (minimum 3-5 components) 
• Component rendering 
• User interactions 
• State changes 
DevOps & Deployment 
6 
Senior Full Stack Developer - Technical Assessment 
1. Containerization (Docker) 
• Dockerfile for the backend API 
• Dockerfile for the frontend application 
• docker-compose.yml to orchestrate: - Backend API - Frontend - Database - Hangfire (if separate) - Redis (if used for caching/SignalR backplane) 
2. CI/CD Pipeline 
Create ONE of the following: 
Option A: GitHub Actions Workflow 
• Build and test automation 
Docker image building and pushing 
• Deployment steps (at least documented) 
Option B: Azure DevOps Pipeline 
• YAML pipeline definition 
• Build, test, and deployment stages 
Option D: Detailed Documentation 
• If you cannot set up an actual pipeline, provide comprehensive documentation: - Step-by-step CI/CD setup guide - Build and test commands - Deployment process - Environment configuration - Infrastructure requirements 
3. Deployment Guidelines Document 
Create a DEPLOYMENT.md file covering: 
7 
Senior Full Stack Developer - Technical Assessment 
• Prerequisites and dependencies 
• Environment variables configuration 
• Database setup and migrations 
• Docker deployment steps 
• Cloud deployment considerations (generic, cloud-agnostic) 
• Scaling considerations 
• Monitoring and logging setup 
• Troubleshooting common issues 
Documentation Requirements 
1. README.md (Mandatory) 
• Project overview and features 
• Technology stack 
• Architecture diagram (visual representation) 
Getting started guide - Prerequisites - Installation steps - Running locally - Running with Docker 
• Testing instructions 
• API documentation (link to Swagger or inline) 
• Screenshots or GIFs of the application 
• Known limitations or future improvements 
2. ARCHITECTURE.md (Mandatory) 
• Detailed architecture explanation 
• Clean Architecture implementation details 
• SOLID principles applied (with code examples/references) 
• Design patterns used 
• Database schema diagram 
• Folder structure explanation 
3. API.md (Optional but Recommended) 
• API endpoints documentation 
• Authentication flow 
• Request/Response examples 
8 
Senior Full Stack Developer - Technical Assessment 
• SignalR hub methods and events 
4. Code Documentation 
• XML comments for public APIs 
• Inline comments for complex logic 
• README files in major folders explaining their purpose 
Functional Requirements 
Minimum Features to Implement 
1. User Management 
• User registration and login 
• JWT authentication 
2. Server/Service Monitoring 
• Add/Edit/Delete monitored servers/services 
• Display list of all servers with current status 
• Server details page with metric history 
3. Metrics Collection (Background Job) 
• Hangfire recurring job simulates metric collection every 1-5 minutes 
• Metrics: CPU %, Memory %, Disk Usage %, Response Time, Status (Up/Down) 
• Store metrics in database with timestamp 
4. Real-Time Dashboard 
• Live updating charts for selected server 
• Real-time notifications when metrics exceed thresholds 
• Auto-refresh when new data arrives via SignalR 
5. Alerting System 
• Define threshold rules (e.g., CPU > 80%, Memory > 90%) 
• Background job checks thresholds and creates alerts 
• Real-time alert notifications to dashboard 
• Alert history view 
9 
6. Report Generation 
Senior Full Stack Developer - Technical Assessment 
• Generate performance reports for a server/time range 
• Fire-and-forget Hangfire job to generate report 
• Report status tracking (Pending, Processing, Completed, Failed) 
• Download/view completed reports 
• Email notification when report is ready (simulate, don't need real email) 
7. Background Jobs Monitoring 
• Display Hangfire dashboard (built-in) 
• Show job execution history 
• Retry failed jobs 
Bonus Points 
These are not required but will be highly valued: 
• Caching Strategy: Redis or in-memory caching for frequently accessed data 
• Health Checks: ASP.NET Core health checks with UI 
• Performance Optimization: Response compression, pagination, lazy loading 
• E2E Tests: Playwright or Cypress tests 
• Infrastructure as Code: Terraform or Bicep templates 
Submission Guidelines 
• Push code to a public repository 
• Ensure repository includes all required documentation 
• Send repository link via email 
Getting Help 
Acceptable 
• Using documentation or StackOverflow 
• Mindful use of AI Tools 
Not Acceptable 
• Copying complete projects or large code sections from tutorials or using AI tools to generate 
the entire application 
• Collaborating with others 
Important Note 
QUALITY OVER QUANTITY: IT'S BETTER TO IMPLEMENT FEWER FEATURES EXCELLENTLY THAN MANY FEATURES POORLY. 
WRITE CODE AS IF THIS WILL GO TO PRODUCTION. CONSIDER SECURITY, PERFORMANCE, AND MAINTAINABILITY. 
10 
Senior Full Stack Developer - Technical Assessment 
AI TOOLS USAGE NEEDS TO BE DEMONSTRATED AND EXPLAINED 
Final Checklist 
Before submitting, ensure you have: 
[ ] Implemented all minimum functional requirements 
[ ] Applied Clean Architecture with clear layer separation 
[ ] Demonstrated SOLID principles with documentation 
[ ] Configured Hangfire with at least 3 job types 
[ ] Implemented real-time updates with SignalR 
[ ] Used EF Core 
[ ] Implemented JWT authentication and authorization 
[ ] Written unit tests with reasonable coverage 
[ ] Written at least 3-5 integration tests 
[ ] Provided CI/CD pipeline or comprehensive deployment documentation 
[ ] Written comprehensive README.md 
[ ] Written detailed ARCHITECTURE.md 
[ ] Written clear DEPLOYMENT.md 
[ ] Added Swagger/OpenAPI documentation 
[ ] Included screenshots or demo video 
[ ] Code is well-commented and readable 
[ ] AI Use Documentation 
11 