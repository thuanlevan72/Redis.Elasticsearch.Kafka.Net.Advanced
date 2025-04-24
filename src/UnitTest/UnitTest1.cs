namespace UnitTest;

/// <summary>
///  định dùng để test hàm nhưng ngại quá nên không làm
/// </summary>
public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var result = 2 + 3;
        Assert.AreEqual(result, 3);
      //  Assert.Pass();
    }
    [Test]
    public void Test2()
    {
        var result = 2 + 3;
        Assert.AreEqual(result, 4);
        //  Assert.Pass();
    }
    [Test]
    public void Test3()
    {
        var result = 2 + 3;
        Assert.AreEqual(result, 5);
    }
}