using System;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infra.Db;

namespace MinimalApi.Domain.Services;

public class VehicleServices : IVehicleServices
{
    private readonly DbContextApp db;

    public VehicleServices(DbContextApp db) {
        this.db = db;
    }

    public void Create(Vehicle vehicle)
    {
        db.Vehicles.Add(vehicle);
        db.SaveChanges();
    }

    public void Delete(Vehicle vehicle)
    {
        db.Vehicles.Remove(vehicle);
        db.SaveChanges();
    }

    public Vehicle? FindById(int id)
    {
        return db.Vehicles.Find(id);
    }

    public List<Vehicle> GetAll(int page = 1, string? name = null, string? brand = null)
    {
        var query = db.Vehicles.AsQueryable();
        if(!string.IsNullOrEmpty(name)) 
        {
            query = query.Where(v => EF.Functions.Like(v.Name.ToLower(), $"%{name}%"));
        }

        var itemsPerPage = 10;

        query = query.Skip((page - 1) * itemsPerPage).Take(itemsPerPage);

        return query.ToList();
    }

    public void Update(Vehicle vehicle)
    {
        db.Vehicles.Update(vehicle);
        db.SaveChanges();
    }
}
