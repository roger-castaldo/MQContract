namespace MQContract.Interfaces.Middleware
{
    /// <summary>
    /// Base Specific Type Middleware just used to limit Generic Types for Register Middleware
    /// </summary>
#pragma warning disable S2326 // Unused type parameters should be removed
    public interface ISpecificTypeMiddleware<TMessage> : IMiddleware
#pragma warning restore S2326 // Unused type parameters should be removed
    {
    }
}
