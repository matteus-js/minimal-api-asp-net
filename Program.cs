var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO loginDTO) => {
    if(loginDTO.Email == "admin@email.com" && loginDTO.Password == "pass1234") {
        return Results.Ok("Login successfully");
    }

    return Results.Unauthorized();
});

app.Run();

public class LoginDTO {
    public string Email {get; set;}
    public string Password {get; set;}
}