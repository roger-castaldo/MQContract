using MQContract;
using MQContract.Messages;

namespace AutomatedTesting
{
    [TestClass]
    public class ErrorMessageTests
    {
        [TestMethod]
        [DataRow(typeof(ObjectDisposedException),"par1","Object Disposed Error",true)]
        [DataRow(typeof(ArgumentNullException), "par1", "Argument Null Error", true)]
        [DataRow(typeof(ArgumentOutOfRangeException), "par1", "Argument Out Of Range Error", true)]
        [DataRow(typeof(OperationCanceledException), null, "Operation Cancelled Error", true)]
        [DataRow(typeof(InvalidOperationException), null, "Invalid Operation Error", true)]
        [DataRow(typeof(TimeoutException), null, "Timeout Error", false)]
        public void TestErrorMessageConstructor(Type exceptionType,string? parameter,string message,bool isFatal)
        {
            #region Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType, (parameter==null ? [message] : [parameter!, message]))!;
            #endregion
            #region Act
            var error = new ErrorMessage(exception);
            #endregion
            #region Assert
            Assert.AreEqual(exception.Message, error.Message);
            Assert.AreEqual(isFatal, error.IsFatal);
            #endregion
            #region Verify
            #endregion
        }

        [TestMethod]
        [DataRow(typeof(ObjectDisposedException), "par1", "Object Disposed Error", true)]
        [DataRow(typeof(ArgumentNullException), "par1", "Argument Null Error", true)]
        [DataRow(typeof(ArgumentOutOfRangeException), "par1", "Argument Out Of Range Error", true)]
        [DataRow(typeof(OperationCanceledException), null, "Operation Cancelled Error", true)]
        [DataRow(typeof(InvalidOperationException), null, "Invalid Operation Error", true)]
        [DataRow(typeof(TimeoutException), null, "Timeout Error", false)]
        public void TestErrorMessageWithTransmissionException(Type exceptionType, string? parameter, string message, bool isFatal)
        {
            #region Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType, (parameter==null ? [message] : [parameter!, message]))!;
            #endregion
            #region Act
            var error = new ErrorMessage(new TransmissionException(exception));
            #endregion
            #region Assert
            Assert.AreEqual(exception.Message, error.Message);
            Assert.AreEqual(isFatal, error.IsFatal);
            #endregion
            #region Verify
            #endregion
        }
    }
}
