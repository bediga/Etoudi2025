using Microsoft.EntityFrameworkCore;
using VcBlazor.Data;
using VcBlazor.Services;
using Syncfusion.Blazor;
using VcBlazor.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure settings from appsettings
var applicationSettings = builder.Configuration.GetSection("Application").Get<ApplicationSettings>() ?? new ApplicationSettings();
var syncfusionSettings = builder.Configuration.GetSection("Syncfusion").Get<SyncfusionSettings>() ?? new SyncfusionSettings();
var securitySettings = builder.Configuration.GetSection("Security").Get<SecuritySettings>() ?? new SecuritySettings();
var featuresSettings = builder.Configuration.GetSection("Features").Get<FeaturesSettings>() ?? new FeaturesSettings();
var databaseSettings = builder.Configuration.GetSection("Database").Get<DatabaseSettings>() ?? new DatabaseSettings();
var cacheSettings = builder.Configuration.GetSection("Cache").Get<CacheSettings>() ?? new CacheSettings();
var performanceSettings = builder.Configuration.GetSection("Performance").Get<PerformanceSettings>() ?? new PerformanceSettings();
var monitoringSettings = builder.Configuration.GetSection("Monitoring").Get<MonitoringSettings>() ?? new MonitoringSettings();
var electionSettings = builder.Configuration.GetSection("Election").Get<ElectionSettings>() ?? new ElectionSettings();

// Register settings as singletons
builder.Services.AddSingleton(applicationSettings);
builder.Services.AddSingleton(syncfusionSettings);
builder.Services.AddSingleton(securitySettings);
builder.Services.AddSingleton(featuresSettings);
builder.Services.AddSingleton(databaseSettings);
builder.Services.AddSingleton(cacheSettings);
builder.Services.AddSingleton(performanceSettings);
builder.Services.AddSingleton(monitoringSettings);
builder.Services.AddSingleton(electionSettings);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add Blazor Server services
builder.Services.AddServerSideBlazor();

// Add Syncfusion Blazor services with license key if available
if (!string.IsNullOrEmpty(syncfusionSettings.LicenseKey))
{
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionSettings.LicenseKey);
}
builder.Services.AddSyncfusionBlazor();

// Add custom services
builder.Services.AddScoped<CsvImportService>();
builder.Services.AddScoped<VcBlazor.Services.IDocumentService, VcBlazor.Services.DocumentService>();
// Add Authentication and Authorization
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Session de 8 heures
        options.SlidingExpiration = true;
        options.Cookie.Name = "VcBlazor.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// Add Identity services
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<VcBlazor.Data.Models.ApplicationUser>, 
    Microsoft.AspNetCore.Identity.PasswordHasher<VcBlazor.Data.Models.ApplicationUser>>();

// Add custom authentication service
builder.Services.AddScoped<VcBlazor.Services.IElectionAuthService, VcBlazor.Services.ElectionAuthService>();

// Add generic permission service
builder.Services.AddScoped<VcBlazor.Services.IPermissionService, VcBlazor.Services.PermissionService>();

// Add Entity Framework Core with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Host=localhost;Database=Vc2025;Username=postgres;Password=yourpassword";
builder.Services.AddDbContext<Vc2025DbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(databaseSettings.CommandTimeout);
        npgsqlOptions.EnableRetryOnFailure(databaseSettings.MaxRetryCount, databaseSettings.RetryDelay, null);
    });
    
    if (databaseSettings.EnableSensitiveDataLogging)
    {
        options.EnableSensitiveDataLogging();
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("VcBlazorPolicy", corsBuilder =>
    {
        if (securitySettings.AllowedOrigins?.Any() == true && securitySettings.AllowedOrigins[0] != "*")
        {
            corsBuilder.WithOrigins(securitySettings.AllowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
        }
        else
        {
            corsBuilder.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
        }
    });
});

// Add Memory Cache if enabled
if (cacheSettings.EnableMemoryCache)
{
    builder.Services.AddMemoryCache();
}

// Add Response Compression if enabled
if (performanceSettings.EnableResponseCompression)
{
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });
}

// Add Health Checks if enabled
if (monitoringSettings.EnableHealthChecks)
{
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "postgresql")
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Enable response compression if configured
if (performanceSettings.EnableResponseCompression)
{
    app.UseResponseCompression();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable CORS with our configured policy
app.UseCors("VcBlazorPolicy");

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Health Checks if enabled
if (monitoringSettings.EnableHealthChecks)
{
    app.MapHealthChecks("/health");
}

// Map Blazor Hub
app.MapBlazorHub();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
