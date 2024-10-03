namespace MQContract.Interfaces.Middleware
{
    /// <summary>
    /// Base Specific Type Middleware just used to limit Generic Types for Register Middleware
    /// </summary>
    public interface ISpecificTypeMiddleware<T>
        where T : class
    {
    }
}
