using MinimalApi.Domain.Entities;

namespace MinimalApi.Domain.Interfaces;


public interface IAdminServices {
    Admin? Login(string email, string password);
    List<Admin> GetAll(int? page = 1);
    Admin? FindById(int id);
    void Create(Admin admin);
}