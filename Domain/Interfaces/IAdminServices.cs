using MinimalApi.Domain.Entities;

namespace MinimalApi.Domain.Interfaces;


public interface IAdminServices {
    Admin? Login(string email, string password);
}