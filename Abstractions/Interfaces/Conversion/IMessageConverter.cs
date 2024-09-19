namespace MQContract.Interfaces.Conversion
{
    /// <summary>
    /// Used to define a message converter.  These are called upon if a 
    /// message is received on a channel of type T but it is waiting for 
    /// message of type V
    /// </summary>
    /// <typeparam name="T">The source message type</typeparam>
    /// <typeparam name="V">The destination message type</typeparam>
    public interface IMessageConverter<in T, V>
    {
        /// <summary>
        /// Called to convert a message from type T to type V
        /// </summary>
        /// <param name="source">The message to convert</param>
        /// <returns>The source message converted to the destination type V</returns>
        ValueTask<V> ConvertAsync(T source);
    }
}
