using VcBlazor.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VcBlazor.Extensions
{
    /// <summary>
    /// Extension methods for configuration services
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Registers all application configuration settings
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration instance</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddApplicationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind and register all configuration sections
            services.Configure<ApplicationSettings>(configuration.GetSection("Application"));
            services.Configure<SyncfusionSettings>(configuration.GetSection("Syncfusion"));
            services.Configure<SecuritySettings>(configuration.GetSection("Security"));
            services.Configure<FeaturesSettings>(configuration.GetSection("Features"));
            services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
            services.Configure<CacheSettings>(configuration.GetSection("Cache"));
            services.Configure<PerformanceSettings>(configuration.GetSection("Performance"));
            services.Configure<MonitoringSettings>(configuration.GetSection("Monitoring"));
            services.Configure<EmailSettings>(configuration.GetSection("Email"));
            services.Configure<StorageSettings>(configuration.GetSection("Storage"));
            services.Configure<ElectionSettings>(configuration.GetSection("Election"));

            return services;
        }

        /// <summary>
        /// Gets strongly typed settings from configuration
        /// </summary>
        /// <typeparam name="T">Settings type</typeparam>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="sectionName">Configuration section name</param>
        /// <returns>Strongly typed settings instance</returns>
        public static T GetSettings<T>(this IConfiguration configuration, string sectionName) where T : new()
        {
            return configuration.GetSection(sectionName).Get<T>() ?? new T();
        }

        /// <summary>
        /// Validates that all required configuration sections are present
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <returns>Validation results</returns>
        public static ConfigurationValidationResult ValidateConfiguration(this IConfiguration configuration)
        {
            var result = new ConfigurationValidationResult { IsValid = true };
            var requiredSections = new[]
            {
                "Application",
                "Database",
                "Logging"
            };

            foreach (var section in requiredSections)
            {
                if (!configuration.GetSection(section).Exists())
                {
                    result.IsValid = false;
                    result.Errors.Add($"Required configuration section '{section}' is missing");
                }
            }

            // Validate connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                result.IsValid = false;
                result.Errors.Add("DefaultConnection connection string is required");
            }

            return result;
        }
    }

    /// <summary>
    /// Configuration validation result
    /// </summary>
    public class ConfigurationValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();

        public void ThrowIfInvalid()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException($"Configuration validation failed: {string.Join(", ", Errors)}");
            }
        }
    }
}