using MinimalApi.Domain.Entities;

namespace Test.Domain.Entities;

[TestClass]
public class  VehicleTest
{
    [TestMethod]
    public void TestGetSetProps()
    {
        // arrange
        var vehicle = new Vehicle();

        // act 
        vehicle.Id = 1;
        vehicle.Name = "Mob";
        vehicle.Brand = "Fiat";
        vehicle.Year = 2020;


        // assertion
        Assert.AreEqual(1, vehicle.Id);
        Assert.AreEqual("Mob", vehicle.Name);
        Assert.AreEqual("Fiat", vehicle.Brand);
        Assert.AreEqual(2020, vehicle.Year);
    }
}