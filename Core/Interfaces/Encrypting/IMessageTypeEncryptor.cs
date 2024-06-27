namespace MQContract.Interfaces.Encrypting
{
    /// <summary>
    /// Used to define a specific message encryptor for the type T.  
    /// This will override the global decryptor if specified for this connection 
    /// as well as the default of not encrypting the message body
    /// </summary>
    /// <typeparam name="T">The type of message that this encryptor supports</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "The generic type here is used to tag an encryptor specific to a message type")]
    public interface IMessageTypeEncryptor<T> : IMessageEncryptor
    {
    }
}
