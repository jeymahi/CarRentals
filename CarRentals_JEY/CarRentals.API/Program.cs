using CarRentals.API.Data;
using CarRentals.API.Models;
using CarRentals.API.RateLimiting;
using CarRentals.API.Security;
using CarRentals.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Adding DbContext with InMemory
builder.Services.AddDbContext<CarRentalContext>(options =>
    options.UseInMemoryDatabase("CarRentalDb"));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adding custom services
builder.Services.AddSingleton(new ApiKeyValidator("secret-key-123")); // you can move to config
builder.Services.AddSingleton(new RateLimiter(10, TimeSpan.FromSeconds(10)));
builder.Services.AddScoped<ReservationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// seed inventory
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<CarRentalContext>();
    if (!ctx.CarInventories.Any())
    {
        ctx.CarInventories.AddRange(
            new CarInventoryEntity { CarType = CarType.Sedan, Capacity = 2 },
            new CarInventoryEntity { CarType = CarType.Suv, Capacity = 1 },
            new CarInventoryEntity { CarType = CarType.Van, Capacity = 1 }
        );
        ctx.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowLocalAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
