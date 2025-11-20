using MQContract.Interfaces.Middleware;

namespace MQContract.Middleware
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true,Inherited=false)]
#pragma warning disable S2326 // Unused type parameters should be removed
    //T is required here as this attribute is used for defining the index in a specific manner for the given type of middleware
    internal class MiddlewareInjectionOrderAttribute<TMiddleware>(int preIndex=0,int postIndex=0) : Attribute
#pragma warning restore S2326 // Unused type parameters should be removed
        where TMiddleware : IMiddleware
    {
        public int GetIndex(MiddlewareCollection.InjectionPositions position)
            => (position == MiddlewareCollection.InjectionPositions.Pre ? preIndex : postIndex);
    }
}
