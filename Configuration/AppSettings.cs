namespace VcBlazor.Configuration
{
    public class ApplicationSettings
    {
        public string Name { get; set; } = "VcBlazor";
        public string Version { get; set; } = "1.0.0";
        public string Environment { get; set; } = "Development";
        public string SupportEmail { get; set; } = "support@vcblazor.com";
        public long MaxFileUploadSize { get; set; } = 10485760; // 10MB
        public int SessionTimeout { get; set; } = 30;
        public bool EnableDemo { get; set; } = true;
    }

    public class SyncfusionSettings
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string Theme { get; set; } = "bootstrap5";
        public bool EnableRtl { get; set; } = false;
        public string Culture { get; set; } = "fr-FR";
    }

    public class SecuritySettings
    {
        public bool RequireHttps { get; set; } = true;
        public bool EnableCors { get; set; } = true;
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public string JwtSecret { get; set; } = string.Empty;
        public int JwtExpirationMinutes { get; set; } = 60;
        public int PasswordMinLength { get; set; } = 8;
        public bool RequireSpecialCharacters { get; set; } = true;
        public int LockoutDuration { get; set; } = 15;
        public int MaxFailedAccessAttempts { get; set; } = 5;
    }

    public class FeaturesSettings
    {
        public bool EnableRealTimeUpdates { get; set; } = true;
        public bool EnableExportToExcel { get; set; } = true;
        public bool EnableEmailNotifications { get; set; } = false;
        public bool EnableSmsNotifications { get; set; } = false;
        public bool EnableAuditLog { get; set; } = true;
        public bool EnableDataArchiving { get; set; } = false;
        public bool MaintenanceMode { get; set; } = false;
    }

    public class DatabaseSettings
    {
        public bool EnableMigrations { get; set; } = true;
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public int CommandTimeout { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
        public bool EnableConnectionPooling { get; set; } = true;
        public int MaxRetryCount { get; set; } = 3;
    }

    public class CacheSettings
    {
        public int DefaultExpirationMinutes { get; set; } = 30;
        public int SlidingExpirationMinutes { get; set; } = 10;
        public bool EnableDistributedCache { get; set; } = false;
        public string CacheKeyPrefix { get; set; } = "VcBlazor:";
        public bool EnableMemoryCache { get; set; } = true;
    }

    public class PerformanceSettings
    {
        public bool EnableResponseCompression { get; set; } = true;
        public bool EnableResponseCaching { get; set; } = true;
        public int MaxConcurrentRequests { get; set; } = 100;
        public int RequestTimeoutSeconds { get; set; } = 30;
        public bool EnableOutputCaching { get; set; } = true;
        public int MaxCircuitsRetained { get; set; } = 100;
    }

    public class MonitoringSettings
    {
        public bool EnableHealthChecks { get; set; } = true;
        public string HealthCheckPath { get; set; } = "/health";
        public bool EnableMetrics { get; set; } = true;
        public string MetricsPath { get; set; } = "/metrics";
        public bool EnableDetailedErrors { get; set; } = true;
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; } = "noreply@vcblazor.com";
        public string FromName { get; set; } = "VcBlazor System";
    }

    public class StorageSettings
    {
        public string DocumentsPath { get; set; } = "Documents";
        public string ImagesPath { get; set; } = "Images";
        public string TempPath { get; set; } = "Temp";
        public long MaxDocumentSize { get; set; } = 5242880; // 5MB
        public string[] AllowedImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        public string[] AllowedDocumentExtensions { get; set; } = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
    }

    public class ElectionSettings
    {
        public int MaxCandidatesPerElection { get; set; } = 50;
        public bool EnableRealTimeResults { get; set; } = true;
        public int ResultsUpdateIntervalSeconds { get; set; } = 30;
        public bool EnableResultsVerification { get; set; } = true;
        public int AutoBackupIntervalMinutes { get; set; } = 60;
        public int MaxPollingStationsPerUser { get; set; } = 10;
    }
}