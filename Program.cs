using Microsoft.EntityFrameworkCore;
using attendance.Data;
using attendance.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS - Allow Everything (Development Only)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterApp", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("*");
    });
});


// Database
builder.Services.AddDbContext<AttendanceDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))));

// Services
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<OfficeLocationService>();
builder.Services.AddScoped<PasswordService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowFlutterApp"); // MUST come BEFORE Authorization
app.UseAuthorization();
app.MapControllers();


// Initialize office location
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<OfficeLocationService>();
    await service.InitializeOfficeLocationAsync();
}

app.Run();
