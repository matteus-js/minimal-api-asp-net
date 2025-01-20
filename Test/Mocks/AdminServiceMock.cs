using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.DTOs;

namespace Test.Mocks;

public class AdminServiceMock : IAdminServices
{
    private static List<Admin> admins = new List<Admin>(){
        new Admin{
            Id = 1,
            Email = "admin@test.com",
            Password = "pass1234",
            Role = "admin"
        },
        new Admin{
            Id = 2,
            Email = "editor@test.com",
            Password = "pass1234",
            Role = "editor"
        }
    };

    public void Create(Admin admin)
    {
        admins.Add(admin);
    }

    public Admin? FindById(int id)
    {
        return admins.Find(admin => admin.Id == id);
    }

    public List<Admin> GetAll(int? page = 1)
    {
        return admins;
    }

    public Admin? Login(string email, string password)
    {
        return admins.Find(admin => admin.Email == email && admin.Password == password);
    }
}