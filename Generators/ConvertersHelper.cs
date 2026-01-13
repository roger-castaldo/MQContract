using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MQContract.Generators
{
    internal static class ConvertersHelper
    {
        internal static IncrementalValuesProvider<ContractConverter?> LocateConverters(IncrementalGeneratorInitializationContext context)
            => context.SyntaxProvider
                .CreateSyntaxProvider<ContractConverter?>(
                    predicate: static (node, _) =>
                        node is ClassDeclarationSyntax cds && cds.BaseList is not null,
                    transform: static (ctx, _) =>
                    {
                        var cds = (ClassDeclarationSyntax)ctx.Node;
                        var symbol = ctx.SemanticModel.GetDeclaredSymbol(cds);

                        if (symbol is not INamedTypeSymbol classSymbol)
                            return null;

                        List<(ITypeSymbol from, ITypeSymbol to)> contracts = [];

                        foreach (var iface in classSymbol.AllInterfaces)
                        {
                            if (!iface.IsGenericType || iface.Name!="IMessageConverter")
                                continue;
                            contracts.Add((iface.TypeArguments[0], iface.TypeArguments[1]));
                        }

                        if (contracts.Count>0)
                            return new ContractConverter(classSymbol, contracts);

                        return null;
                    }
                )
                .Where(static s => s is not null)!;
    }
}
