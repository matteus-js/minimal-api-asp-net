using MinimalApi.Domain.Entities;

namespace MinimalApi.Domain.Interfaces;


public interface IVehicleServices {
    List<Vehicle> GetAll(int? page = 1, string? name = null, string? brand = null);
    Vehicle? FindById(int id);
    void Create(Vehicle vehicle);
    void Update(Vehicle vehicle);
    void Delete(Vehicle vehicle);
}