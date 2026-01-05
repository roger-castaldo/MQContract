using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces.Encrypting;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace MQContract
{
    internal partial class MessageContext
    {
        internal IMessageEncryptor? GetMessageEncryptor(Type messageType, IMessageEncryptor? globalEncryptor, IServiceProvider? serviceProvider)
        {
            foreach(var context in contexts)
            {
                var result = context.TryGetMessageEncryptor(messageType, globalEncryptor, serviceProvider);
                if (result!=null)
                    return result;
            }
            if (RuntimeFeature.IsDynamicCodeSupported)
                return ExtractEncryptorThroughReflection(messageType, globalEncryptor, serviceProvider);
            return null;
        }

        [RequiresDynamicCode("Uses unbounded reflection to discover encryptors, if AOT and no usage of UseMqContractAttribute to autogenerate code, will result in falling back to Non Encrypting as default")]
        private static IMessageEncryptor? ExtractEncryptorThroughReflection(Type messageType, IMessageEncryptor? globalEncryptor, IServiceProvider? serviceProvider)
        {
            var ifaceType = typeof(IMessageTypeEncryptor<>).MakeGenericType(messageType);
            var encryptorType = AssemblyLoadContext.All
                .SelectMany(context => context.Assemblies)
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes()
                        .Where(t => !t.IsInterface && !t.IsAbstract
                            && Array.Exists(t.GetInterfaces(), iface => Equals(iface, ifaceType)));
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                })
                .FirstOrDefault();
            if (encryptorType!=null)
                return (IMessageEncryptor)(serviceProvider!=null ? ActivatorUtilities.CreateInstance(serviceProvider!, encryptorType!)! : Activator.CreateInstance(encryptorType!)!);
            return globalEncryptor;
        }
    }
}
