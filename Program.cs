using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infra.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdminServices, AdminServices>();
builder.Services.AddDbContext<DbContextApp>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdminServices adminServices) => {
    return adminServices.Login(loginDTO.Email, loginDTO.Password) != null ? Results.Ok("Login successfully") : Results.Unauthorized();
});

app.Run();