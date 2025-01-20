using MinimalApi.Domain.Entities;

namespace Test.Domain.Entities;

[TestClass]
public class AdminTest
{
    [TestMethod]
    public void TestGetSetProps()
    {
        // arrange
        var admin = new Admin();

        // act 
        admin.Id = 1;
        admin.Email = "admin@test.com";
        admin.Password = "pass1234";

        // assertion
        Assert.AreEqual(1, admin.Id);
        Assert.AreEqual("admin@test.com", admin.Email);
        Assert.AreEqual("pass1234", admin.Password);

    }
}