using Microsoft.EntityFrameworkCore;
using ServerMonitoring.Application.Interfaces;
using ServerMonitoring.Domain.Entities;
using ServerMonitoring.Domain.Enums;
using ServerMonitoring.Infrastructure.Data;

namespace ServerMonitoring.Infrastructure.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IAuthService authService)
    {
        // Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role
                {
                    Name = "Admin",
                    Description = "Administrator with full access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Role
                {
                    Name = "User",
                    Description = "Regular user with limited access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Role
                {
                    Name = "Operator",
                    Description = "System operator with monitoring access",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
        }

        // Seed Users
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@servermonitoring.com",
                PasswordHash = authService.HashPassword("Admin@123"),
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            var regularUser = new User
            {
                Username = "user",
                Email = "user@servermonitoring.com",
                PasswordHash = authService.HashPassword("User@123"),
                FirstName = "Regular",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.Users.AddRange(adminUser, regularUser);
            await context.SaveChangesAsync();

            // Assign roles
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var userRole = await context.Roles.FirstAsync(r => r.Name == "User");

            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new UserRole
                {
                    UserId = regularUser.Id,
                    RoleId = userRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            context.UserRoles.AddRange(userRoles);
            await context.SaveChangesAsync();
        }

        // Seed Servers
        if (!await context.Servers.AnyAsync())
        {
            var servers = new List<Server>
            {
                new Server
                {
                    Name = "Web Server 01",
                    Hostname = "web-01.company.com",
                    IPAddress = "192.168.1.10",
                    Port = 80,
                    OperatingSystem = "Windows Server 2022",
                    Status = ServerStatus.Online,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Server
                {
                    Name = "Database Server",
                    Hostname = "db-01.company.com",
                    IPAddress = "192.168.1.20",
                    Port = 1433,
                    OperatingSystem = "Windows Server 2022",
                    Status = ServerStatus.Online,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Server
                {
                    Name = "Application Server",
                    Hostname = "app-01.company.com",
                    IPAddress = "192.168.1.30",
                    Port = 8080,
                    OperatingSystem = "Linux Ubuntu 22.04",
                    Status = ServerStatus.Online,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            context.Servers.AddRange(servers);
            await context.SaveChangesAsync();

            // Seed Metrics for servers
            var random = new Random();
            foreach (var server in servers)
            {
                var metrics = new List<Metric>();
                for (int i = 0; i < 10; i++)
                {
                    metrics.Add(new Metric
                    {
                        ServerId = server.Id,
                        CpuUsage = random.Next(10, 90),
                        MemoryUsage = random.Next(20, 80),
                        DiskUsage = random.Next(30, 70),
                        NetworkInbound = random.Next(5, 50),
                        NetworkOutbound = random.Next(5, 50),
                        ResponseTime = random.Next(10, 200),
                        Timestamp = DateTime.UtcNow.AddMinutes(-i * 5),
                        CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5)
                    });
                }
                context.Metrics.AddRange(metrics);
            }
            await context.SaveChangesAsync();

            // Seed Disks
            var disks = new List<Disk>
            {
                new Disk
                {
                    ServerId = servers[0].Id,
                    DriveLetter = "C:",
                    Label = "System",
                    TotalSizeBytes = 500L * 1024 * 1024 * 1024, // 500 GB
                    FreeSpaceBytes = 250L * 1024 * 1024 * 1024, // 250 GB
                    FileSystem = "NTFS",
                    IsReady = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Disk
                {
                    ServerId = servers[1].Id,
                    DriveLetter = "C:",
                    Label = "System",
                    TotalSizeBytes = 1000L * 1024 * 1024 * 1024, // 1 TB
                    FreeSpaceBytes = 300L * 1024 * 1024 * 1024, // 300 GB
                    FileSystem = "NTFS",
                    IsReady = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };

            context.Disks.AddRange(disks);
            await context.SaveChangesAsync();

            // Seed Alerts
            var adminUserId = (await context.Users.FirstAsync(u => u.Username == "admin")).Id;
            var alerts = new List<Alert>
            {
                new Alert
                {
                    ServerId = servers[0].Id,
                    UserId = adminUserId,
                    Type = AlertType.CpuUsage,
                    Severity = AlertSeverity.Warning,
                    Title = "High CPU Usage",
                    Message = "CPU usage exceeded 80% threshold",
                    ThresholdValue = 80,
                    ActualValue = 85,
                    IsAcknowledged = false,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    CreatedBy = "System"
                },
                new Alert
                {
                    ServerId = servers[1].Id,
                    UserId = adminUserId,
                    Type = AlertType.DiskSpace,
                    Severity = AlertSeverity.Critical,
                    Title = "Low Disk Space",
                    Message = "Disk space below 20% threshold",
                    ThresholdValue = 20,
                    ActualValue = 15,
                    IsAcknowledged = true,
                    AcknowledgedAt = DateTime.UtcNow.AddHours(-1),
                    AcknowledgedBy = "admin",
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    CreatedBy = "System"
                }
            };

            context.Alerts.AddRange(alerts);
            await context.SaveChangesAsync();

            // Seed Reports
            var reports = new List<Report>
            {
                new Report
                {
                    Title = "Daily Metrics Report",
                    Description = "Daily server performance metrics",
                    Type = ReportType.DailyMetrics,
                    Status = ReportStatus.Completed,
                    StartDate = DateTime.UtcNow.Date.AddDays(-1),
                    EndDate = DateTime.UtcNow.Date,
                    FilePath = "/reports/daily-metrics-2026-02-09.pdf",
                    FileFormat = "PDF",
                    FileSizeBytes = 1024 * 512, // 512 KB
                    GeneratedByUserId = adminUserId,
                    GeneratedAt = DateTime.UtcNow.AddHours(-1),
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    CreatedBy = "System"
                },
                new Report
                {
                    Title = "Weekly Alert Summary",
                    Description = "Summary of alerts from the past week",
                    Type = ReportType.AlertSummary,
                    Status = ReportStatus.Processing,
                    StartDate = DateTime.UtcNow.Date.AddDays(-7),
                    EndDate = DateTime.UtcNow.Date,
                    GeneratedByUserId = adminUserId,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                    CreatedBy = "System"
                }
            };

            context.Reports.AddRange(reports);
            await context.SaveChangesAsync();
        }
    }
}
