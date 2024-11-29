using Microsoft.EntityFrameworkCore;
using MinimalApi.DTOs;
using MinimalApi.Infra.Db;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DbContextApp>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) => {
    if(loginDTO.Email == "admin@email.com" && loginDTO.Password == "pass1234") {
        return Results.Ok("Login successfully");
    }

    return Results.Unauthorized();
});

app.Run();