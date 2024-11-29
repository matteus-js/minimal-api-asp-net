using System;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infra.Db;

namespace MinimalApi.Domain.Services;

public class AdminServices : IAdminServices
{
    private readonly DbContextApp db;

    public AdminServices(DbContextApp db) {
        this.db = db;
    }
    Admin? IAdminServices.Login(string email, string password)
    {
        var admin = db.Admins.Where(admin => admin.Email == email && admin.Password == password).FirstOrDefault();
        return admin;
    }
}
