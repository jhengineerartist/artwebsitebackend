using ArtWebsiteDataAccess;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Events;

DotNetEnv.Env.Load();

// Create a WebApplicationBuilder instance
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

// Configure Serilog using the logging builder
builder.Logging.Services.AddSerilog((context, configuration) =>
{
    // Configure Serilog using the context and configuration parameters
    configuration
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console(LogEventLevel.Information, "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/log.txt", LogEventLevel.Debug, rollingInterval: RollingInterval.Day);
});

// Configure the blob storage credential
var credential = new ClientSecretCredential(
    Environment.GetEnvironmentVariable("ENTRA_APP_TENANT_ID"),
    Environment.GetEnvironmentVariable("ENTRA_APP_ID"),
    Environment.GetEnvironmentVariable("ENTRA_APP_SECRET_VALUE")
);

var blobServiceClient = new BlobServiceClient(new Uri($"https://{Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT")}.blob.core.windows.net"), credential);

// Create a SqlConnectionStringBuilder object and set its properties 
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

var connection = new SqlConnection(connectionString);

builder.Services.AddSingleton(credential);
builder.Services.AddSingleton(blobServiceClient);
builder.Services.AddSingleton(connection);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "PrimaryCors",
        corsBuilder =>
        {
            // Set the origins based on the environment
            if (builder.Environment.IsDevelopment())
            {
                // Allow any origin in development
                Console.WriteLine("Allowing Any Origin.");
                corsBuilder.AllowAnyOrigin();
            }
            else
            {
                // Allow only jhirshman.art in production
                Console.WriteLine("Allowing only the production origin.");
                corsBuilder.WithOrigins("https://jhirshman.art");
            }

            // Allow any header and method
            corsBuilder.AllowAnyHeader()
                    .AllowAnyMethod();
        });
});

// Add the ImageDbContext class as a service 
builder.Services.AddDbContext<ImageDbContext>(options =>
{ // Use Azure SQL Database as the database provider
    options.UseSqlServer(connection);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Use CORS middleware with the policy name
app.UseCors("PrimaryCors");

Log.Information($"Connection {connection.Database} connected successfully.");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())

{
    Log.Information($"Running In Development Evironment");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    Log.Information($"Running In Production Evironment");
    app.UseExceptionHandler("/Error");
    // Enable HTTPS redirection and HSTS middleware
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();