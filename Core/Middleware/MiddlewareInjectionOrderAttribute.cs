using MQContract.Interfaces.Middleware;

namespace MQContract.Middleware
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true,Inherited=false)]
    internal class MiddlewareInjectionOrderAttribute<T>(int preIndex=0,int postIndex=0) : Attribute
        where T : IMiddleware
    {
        public int GetIndex(MiddlewareCollection.InjectionPositions position)
            => (position == MiddlewareCollection.InjectionPositions.Pre ? preIndex : postIndex);
    }
}
