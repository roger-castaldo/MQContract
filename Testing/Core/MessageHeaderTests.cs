namespace CoreTesting;

[TestClass]
public class MessageHeaderTests
{
    private static readonly string key1 = Helper.GenerateRandomString(20);
    private static readonly string value1 = Helper.GenerateRandomString(20);
    private static readonly string key2 = Helper.GenerateRandomString(20);
    private static readonly string value2 = Helper.GenerateRandomString(20);
    private static readonly string key3 = Helper.GenerateRandomString(20);
    private const string value3 = "";

    [TestMethod]
    public void TestEnumerableConstructor()
    {
        #region Arrange
        var header = new MessageHeader([
            new(key1,value1),
            new(key2,value2)
        ]);
        #endregion

        #region Act
        #endregion

        #region Assert
        Assert.HasCount(2, header.Keys);
        Assert.Contains(key1, header.Keys);
        Assert.Contains(key2, header.Keys);
        Assert.AreEqual(value1, header[key1]);
        Assert.AreEqual(value2, header[key2]);
        #endregion

        #region Verify
        #endregion
    }

    [TestMethod]
    public void TestDictionaryConstructor()
    {
        #region Arrange
        var header = new MessageHeader(new Dictionary<string, string?>([
            new(key1,value1),
            new(key2,value2),
            new(key3,value3)
        ]));
        #endregion

        #region Act
        #endregion

        #region Assert
        Assert.AreEqual(3, header.Count);
        Assert.Contains(key1, header.Keys);
        Assert.Contains(key2, header.Keys);
        Assert.Contains(key3, header.Keys);
        Assert.AreEqual(value1, header[key1]);
        Assert.AreEqual(value2, header[key2]);
        Assert.AreEqual(string.Empty, header[key3]);
        #endregion

        #region Verify
        #endregion
    }

    [TestMethod]
    public void TestAppendDictionaryConstructorWithoutOverride()
    {
        #region Arrange
        var originalHeader = new MessageHeader(new Dictionary<string, string?>([
            new(key1,value1),
            new(key3,value3)
        ]));
        var header = new MessageHeader(originalHeader, new Dictionary<string, string?>([
            new(key2,value2)
        ]));
        #endregion

        #region Act
        #endregion

        #region Assert
        Assert.AreEqual(3, header.Count);
        Assert.Contains(key1, header.Keys);
        Assert.Contains(key2, header.Keys);
        Assert.Contains(key3, header.Keys);
        Assert.AreEqual(value1, header[key1]);
        Assert.AreEqual(value2, header[key2]);
        Assert.AreEqual(string.Empty, header[key3]);
        #endregion

        #region Verify
        #endregion
    }

    [TestMethod]
    public void TestAppendDictionaryConstructorWithOverride()
    {
        #region Arrange
        var originalHeader = new MessageHeader(new Dictionary<string, string?>([
            new(key1,value1),
            new(key2,value1),
            new(key3,value3)
        ]));
        var header = new MessageHeader(originalHeader, new Dictionary<string, string?>([
            new(key2,value2)
        ]));
        #endregion

        #region Act
        #endregion

        #region Assert
        Assert.HasCount(3, header.Keys);
        Assert.Contains(key1, header.Keys);
        Assert.Contains(key2, header.Keys);
        Assert.Contains(key3, header.Keys);
        Assert.AreEqual(value1, header[key1]);
        Assert.AreEqual(value2, header[key2]);
        Assert.AreEqual(string.Empty, header[key3]);
        #endregion

        #region Verify
        #endregion
    }
}
