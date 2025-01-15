using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infra.Db;

#region  builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString() ?? "123456";

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( options => 
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Bearer {token}"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference 
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
);

builder.Services.AddScoped<IAdminServices, AdminServices>();
builder.Services.AddScoped<IVehicleServices, VehicleServices>();
builder.Services.AddDbContext<DbContextApp>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});
var app = builder.Build();
#endregion

app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");

string GenerateTokenJwt(Admin admin) {
    var securityKey = new  SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>() {
        new Claim("Email", admin.Email),
        new Claim("Role", admin.Role),
        new Claim(ClaimTypes.Role, admin.Role)

    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    ); 

    return new JwtSecurityTokenHandler().WriteToken(token);
} 

#region  admin
app.MapPost("/admins/login", ([FromBody] LoginDTO loginDTO, IAdminServices adminServices) =>
{
    var admin = adminServices.Login(loginDTO.Email, loginDTO.Password);
    if(admin == null) 
    {
        return Results.Unauthorized();
    }
    var token = GenerateTokenJwt(admin);
    return Results.Ok(
        new {
        Token = token
    });
}).WithTags("Admin");

app.MapPost("/admins", ([FromBody] AdminDTO adminDTO, IAdminServices adminServices) =>
{
    ValidateErrors validateErrors = new ValidateErrors{
        Messages = new List<string>()
    };
        if (string.IsNullOrEmpty(adminDTO.Email))
        {
            validateErrors.Messages.Add("\"email\" não pode ser vazio.");
        }
        if (string.IsNullOrEmpty(adminDTO.Password))
        {
            validateErrors.Messages.Add("\"password\" não pode ser vazia.");
        }
        if (adminDTO.Role != null && (adminDTO.Role != "admin" || adminDTO.Role != "editor"))
        {
            validateErrors.Messages.Add("\"role\" deve ser \"admin\" ou \"editor\".");
        }
        if(validateErrors.Messages.Count > 0) return Results.BadRequest(validateErrors);
    Admin admin = new()
    {
        Email = adminDTO.Email,
        Password = adminDTO.Password,
        Role = adminDTO.Role ?? "editor"
    };
    adminServices.Create(admin);

    return Results.Created($"/admins/{admin.Id}", new AdminModelView{Id = admin.Id, Email = admin.Email, Role = admin.Role});
})
    .RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin"})
    .WithTags("Admin");

app.MapGet("/admins/{id}", ([FromRoute] int id , IAdminServices adminServices) =>
{
    return adminServices.FindById(id)
        is Admin admin
        ? Results.Ok(new AdminModelView{Id = admin.Id, Email = admin.Email, Role = admin.Role})
        : Results.NotFound();
})
    .RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin"})
    .WithTags("Admin");

app.MapGet("/admins", ([FromQuery] int? page , IAdminServices adminServices) =>
{
    List<Admin> admins = adminServices.GetAll();
    List<AdminModelView> listAdminModelView = [];
    foreach (var admin in admins)
    { 
      listAdminModelView.Add(
        new AdminModelView 
        {
            Id = admin.Id,
            Email = admin.Email,
            Role = admin.Role
        }
      );
    }
    return Results.Ok(listAdminModelView);
    
})
    .RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin"})
    .WithTags("Admin");
#endregion

#region  vehicles
    ValidateErrors ValidateVehicleDTO(VehicleDTO vehicleDTO) 
    {
        if (vehicleDTO == null)
        {
            throw new ArgumentNullException(nameof(vehicleDTO), "O objeto VehicleDTO não pode ser nulo.");
        }
        ValidateErrors validateErrors = new ValidateErrors();
        if (string.IsNullOrEmpty(vehicleDTO.Name))
        {
            validateErrors.Messages.Add("Nome não pode ser vazia");
        }
        if (string.IsNullOrEmpty(vehicleDTO.Brand))
        {
            validateErrors.Messages.Add("Brand não pode ser vazia");
        }
        if (vehicleDTO.Year < 1950)
        {
            validateErrors.Messages.Add("Year deve ser igual ou maior que 1950");
        }
        return validateErrors;
    }

app.MapPost("/vehicles/", ([FromBody] VehicleDTO vehicleDTO, IVehicleServices vehicleServices) =>
{

    var validateErrors = ValidateVehicleDTO(vehicleDTO);

    if (validateErrors.Messages.Count > 0)
    {
        return Results.BadRequest(validateErrors);
    }
    Vehicle vehicle = new Vehicle
    {
        Name = vehicleDTO.Name,
        Brand = vehicleDTO.Brand,
        Year = vehicleDTO.Year
    };

    vehicleServices.Create(vehicle);

    return Results.Created($"/vehicles/{vehicle.Id}", vehicle);
})
    .RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin"})
    .WithTags("Vehicles");

app.MapGet("/vehicles/", ([FromQuery] int? page, IVehicleServices vehicleServices) =>
{
    return vehicleServices.GetAll(page);
})
    .RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin, editor"})
    .WithTags("Vehicles");

app.MapGet("/vehicles/{id}", (int id, IVehicleServices vehicleServices) =>
{
    return vehicleServices.FindById(id)
        is Vehicle vehicle
        ? Results.Ok(vehicle)
        : Results.NotFound();
})
    .RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin, editor"})
    .WithTags("Vehicles");

app.MapPut("/vehicles/{id}", ([FromRoute] int id, [FromBody] VehicleDTO vehicleDTO, IVehicleServices vehicleServices) =>
{
    Console.WriteLine(vehicleDTO != null ? "vehicleDTO está inicializado" : "vehicleDTO é nulo");
    var validateErrors = ValidateVehicleDTO(vehicleDTO);

    if (validateErrors.Messages.Count > 0)
    {
        return Results.BadRequest(validateErrors);
    }

    var vehicle = vehicleServices.FindById(id);
    if (vehicle is null) return Results.NotFound();
    vehicle.Name = vehicleDTO.Name;
    vehicle.Brand = vehicleDTO.Brand;
    vehicle.Year = vehicleDTO.Year;
    vehicleServices.Update(vehicle);
    return Results.NoContent();
})
    .RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin, editor"})
    .WithTags("Vehicles");

app.MapDelete("/vehicles/{id}", (int id, IVehicleServices vehicleServices) =>
{
    if (vehicleServices.FindById(id) is Vehicle vehicle)
    {
        vehicleServices.Delete(vehicle);
        return Results.NoContent();
    }

    return Results.NotFound();
})  
    .RequireAuthorization()
    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin"})
    .WithTags("Vehicles");

#endregion

#region  app
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

#endregion