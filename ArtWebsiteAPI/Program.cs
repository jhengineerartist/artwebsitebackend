using ArtWebsiteAPI;
using Serilog;
using ArtWebsiteDataAccess;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Data.SqlClient;

DotNetEnv.Env.Load();

// Create a WebApplicationBuilder instance
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

InitializationUtils.ConfigureSerilog(builder);

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

// Add the ImageDbContext class as a service 
builder.Services.AddDbContext<ImageDbContext>(options =>
{ // Use Azure SQL Database as the database provider
    options.UseSqlServer(connection);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

Log.Information($"Connection {connection.Database} connected successfully.");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    Log.Information($"Running In Development Evironment");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();