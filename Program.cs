using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infra.Db;

#region  builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAdminServices, AdminServices>();
builder.Services.AddScoped<IVehicleServices, VehicleServices>();
builder.Services.AddDbContext<DbContextApp>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});
var app = builder.Build();
#endregion

app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");

#region  admin
app.MapPost("/admins/login", ([FromBody] LoginDTO loginDTO, IAdminServices adminServices) => {
    return adminServices.Login(loginDTO.Email, loginDTO.Password) != null ? Results.Ok("Login successfully") : Results.Unauthorized();
}).WithTags("Admin");
#endregion

#region  vehicles
app.MapPost("/vehicles/", ([FromBody] VehicleDTO vehicleDTO, IVehicleServices vehicleServices) => {
    Vehicle vehicle = new Vehicle {
        Name = vehicleDTO.Name,
        Brand = vehicleDTO.Brand,
        Year = vehicleDTO.Year
    };

    vehicleServices.Create(vehicle);

    return Results.Created($"/vehicles/{vehicle.Id}", vehicle);
}).WithTags("Vehicles");

app.MapGet("/vehicles/", ([FromQuery] int? page, IVehicleServices vehicleServices) => {
    return vehicleServices.GetAll(page);
}).WithTags("Vehicles");

app.MapGet("/vehicles/{id}", (int id, IVehicleServices vehicleServices) => {
    return vehicleServices.FindById(id)
        is Vehicle vehicle
        ? Results.Ok(vehicle)
        : Results.NotFound();
}).WithTags("Vehicles");

app.MapPut("/vehicles/{id}", ([FromRoute]int id, [FromBody] VehicleDTO vehicleDTO, IVehicleServices vehicleServices) => {
    var vehicle = vehicleServices.FindById(id);
    if(vehicle is null) return Results.NotFound();
    vehicle.Name = vehicleDTO.Name;
    vehicle.Brand = vehicleDTO.Brand;
    vehicle.Year = vehicleDTO.Year;
    vehicleServices.Update(vehicle);
    return Results.NoContent();
}).WithTags("Vehicles");

app.MapDelete("/vehicles/{id}", (int id, IVehicleServices vehicleServices) => {
    if(vehicleServices.FindById(id) is Vehicle vehicle)
    {
        vehicleServices.Delete(vehicle);
        return Results.NoContent();
    }

    return Results.NotFound();
}).WithTags("Vehicles");

#endregion

#region  app
app.UseSwagger();
app.UseSwaggerUI();

app.Run();

#endregion