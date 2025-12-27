using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace MQContract.Generators
{
    internal static class Helper
    {
        public static (IEnumerable<IAssemblySymbol> assemblies, INamedTypeSymbol[] messages) ExtractMessageTypes(Compilation compilation)
        {
            var assemblies = compilation.SourceModule
                            .ReferencedAssemblySymbols
                            .Append(compilation.Assembly);

            // Collect message types
            var messages = assemblies.SelectMany(assembly =>
                assembly.GetAttributes()
                .Where(attr => attr.AttributeClass?.Name == "UseMqContractAttribute" &&
                        attr.ConstructorArguments[0].Value is INamedTypeSymbol)
                .Select(attr => (INamedTypeSymbol)attr.ConstructorArguments[0].Value!)
            )
                .Distinct(SymbolEqualityComparer.Default)
                .OfType<INamedTypeSymbol>()
                .ToArray();
            return (assemblies, messages);
        }
    }
}
