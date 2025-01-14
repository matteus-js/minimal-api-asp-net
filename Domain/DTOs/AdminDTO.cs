using System.ComponentModel.DataAnnotations;

namespace MinimalApi.DTOs;
public class AdminDTO {
    public string Email {get; set;} = default!;

    public string Password  {get; set;} = default!;
    
    public string? Role  {get; set;} = default!;
}