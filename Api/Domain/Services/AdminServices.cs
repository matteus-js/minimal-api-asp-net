using System;
using System.Reflection.Metadata.Ecma335;
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

    public void Create(Admin admin)
    {
        db.Admins.Add(admin);
        db.SaveChanges();
    }

    public Admin? FindById(int id)
    {
        return db.Admins.Find(id);
    }

    public List<Admin> GetAll(int? page = 1)
    {
        if(page != null) {
            return db.Admins.Skip(((int)page - 1)*10).ToList();
        }
        return db.Admins.ToList();
    }

    Admin? IAdminServices.Login(string email, string password)
    {
        var admin = db.Admins.Where(admin => admin.Email == email && admin.Password == password).FirstOrDefault();
        return admin;
    }
}
