using Microsoft.EntityFrameworkCore;
using attendance.Data;
using attendance.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure CORS for Flutter app
// In production, restrict this to specific origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterApp",
        policy =>
        {
            policy.AllowAnyOrigin()  // For development - restrict in production
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add Entity Framework with MySQL
builder.Services.AddDbContext<AttendanceDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        new MySqlServerVersion(new Version(8, 0, 21)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Register services
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<OfficeLocationService>();
builder.Services.AddScoped<PasswordService>();

// Add controllers (API only, no views)
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFlutterApp");

// Enable static files for uploaded images
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Initialize office location from appsettings.json on startup
using (var scope = app.Services.CreateScope())
{
    var officeLocationService = scope.ServiceProvider.GetRequiredService<OfficeLocationService>();
    try
    {
        await officeLocationService.InitializeOfficeLocationAsync();
        Console.WriteLine("Office location initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not initialize office location: {ex.Message}");
    }
}

app.Run();
