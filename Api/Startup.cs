using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infra.Db;

namespace MinimalApi;
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
    }

    private string key = "";
    public IConfiguration Configuration { get;set; } = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(options => {
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

        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen( options => 
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

        services.AddScoped<IAdminServices, AdminServices>();
        services.AddScoped<IVehicleServices, VehicleServices>();
        services.AddDbContext<DbContextApp>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("MySql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
            );
        });

         services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors();

            app.UseEndpoints(endpoints => {

                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
                #endregion

                #region Administradores
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

                endpoints.MapPost("/admins/login", ([FromBody] LoginDTO loginDTO, IAdminServices adminServices) =>
                {
                    var admin = adminServices.Login(loginDTO.Email, loginDTO.Password);
                    if(admin == null) 
                    {

                        return Results.Unauthorized();
                    }
                    var token = GenerateTokenJwt(admin);
                    return Results.Ok(
                        new AdminLoginModelView{
                        Token = token
                    });
                }).WithTags("Admin");

                endpoints.MapPost("/admins", ([FromBody] AdminDTO adminDTO, IAdminServices adminServices) =>
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

                endpoints.MapGet("/admins/{id}", ([FromRoute] int id , IAdminServices adminServices) =>
                {
                    return adminServices.FindById(id)
                        is Admin admin
                        ? Results.Ok(new AdminModelView{Id = admin.Id, Email = admin.Email, Role = admin.Role})
                        : Results.NotFound();
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin"})
                    .WithTags("Admin");

                endpoints.MapGet("/admins", ([FromQuery] int? page , IAdminServices adminServices) =>
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

                endpoints.MapPost("/vehicles/", ([FromBody] VehicleDTO vehicleDTO, IVehicleServices vehicleServices) =>
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

                endpoints.MapGet("/vehicles/", ([FromQuery] int? page, IVehicleServices vehicleServices) =>
                {
                    return vehicleServices.GetAll(page);
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin, editor"})
                    .WithTags("Vehicles");

                endpoints.MapGet("/vehicles/{id}", (int id, IVehicleServices vehicleServices) =>
                {
                    return vehicleServices.FindById(id)
                        is Vehicle vehicle
                        ? Results.Ok(vehicle)
                        : Results.NotFound();
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "admin, editor"})
                    .WithTags("Vehicles");

                endpoints.MapPut("/vehicles/{id}", ([FromRoute] int id, [FromBody] VehicleDTO vehicleDTO, IVehicleServices vehicleServices) =>
                {
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

                endpoints.MapDelete("/vehicles/{id}", (int id, IVehicleServices vehicleServices) =>
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
        });
    }
}