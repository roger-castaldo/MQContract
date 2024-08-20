using MQContract.Interfaces.Service;
using System.Diagnostics.CodeAnalysis;

namespace MQContract
{
    /// <summary>
    /// An exception thrown when the options supplied to an underlying system connection are not of an expected type.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification ="This exception is only really thrown from underlying service connection implementations")]
    public sealed class InvalidChannelOptionsTypeException
        : InvalidCastException
    {
        internal InvalidChannelOptionsTypeException(IEnumerable<Type> expectedTypes,Type recievedType) : 
            base($"Expected Channel Options of Types[{string.Join(',',expectedTypes.Select(t=>t.FullName))}] but recieved {recievedType.FullName}")
        {
        }

        
        internal InvalidChannelOptionsTypeException(Type expectedType, Type recievedType) :
            base($"Expected Channel Options of Type {expectedType.FullName} but recieved {recievedType.FullName}")
        {
        }

        /// <summary>
        /// Called to check if the options is of a given type
        /// </summary>
        /// <typeparam name="T">The expected type for the ServiceChannelOptions</typeparam>
        /// <param name="options">The supplied service channel options</param>
        /// <exception cref="InvalidChannelOptionsTypeException">Thrown when the options value is not null and not of type T</exception>
        public static void ThrowIfNotNullAndNotOfType<T>(IServiceChannelOptions? options)
        {
            if (options!=null && options is not T)
                throw new InvalidChannelOptionsTypeException(typeof(T), options.GetType());
        }

        /// <summary>
        /// Called to check if the options is one of the given types
        /// </summary>
        /// <param name="options">The supplied service channel options</param>
        /// <param name="expectedTypes">The possible types it can be</param>
        /// <exception cref="InvalidChannelOptionsTypeException">Thrown when the options value is not null and not of any of the expected Types</exception>
        public static void ThrowIfNotNullAndNotOfType(IServiceChannelOptions? options,IEnumerable<Type> expectedTypes)
        {
            if (options!=null && !expectedTypes.Contains(options.GetType()))
                throw new InvalidChannelOptionsTypeException(expectedTypes, options.GetType());
        }
    }

    /// <summary>
    /// An exception thrown when there are options supplied to an underlying system connection that does not support options for that particular instance
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "This exception is only really thrown from underlying service connection implementations")]
    public sealed class NoChannelOptionsAvailableException
        : Exception
    {
        internal NoChannelOptionsAvailableException()
            : base("There are no service channel options available for this action") { }

        /// <summary>
        /// Called to throw if options is not null
        /// </summary>
        /// <param name="options">The service channel options that were supplied</param>
        /// <exception cref="NoChannelOptionsAvailableException">Thrown when the options is not null</exception>
        public static void ThrowIfNotNull(IServiceChannelOptions? options)
        {
            if (options!=null)
                throw new NoChannelOptionsAvailableException();
        }
    }
}
