using CarRentals.API.Data;
using CarRentals.API.Interface;
using CarRentals.API.Middleware;
using CarRentals.API.Models;
using CarRentals.API.Security;
using CarRentals.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Adding DbContext with InMemory
builder.Services.AddDbContext<CarRentalContext>(options =>
    options.UseInMemoryDatabase("CarRentalDb"));

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adding custom services
builder.Services.AddSingleton(new ApiKeyValidator("secret-key-123")); // you can move to config
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddSingleton<RateLimiter>(_ =>
    new RateLimiter(5, TimeSpan.FromMinutes(1)));
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

app.UseReservationRateLimiting();

app.MapControllers();

app.Run();
