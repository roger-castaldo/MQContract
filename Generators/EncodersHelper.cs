using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MQContract.Generators
{
    internal static class EncodersHelper
    {
        public static IncrementalValuesProvider<ContractEncoder?> LocateEncoders(IncrementalGeneratorInitializationContext context)
            => context.SyntaxProvider
                .CreateSyntaxProvider<ContractEncoder?>(
                    predicate: static (node, _) =>
                        node is ClassDeclarationSyntax cds && cds.BaseList is not null,
                    transform: static (ctx, _) =>
                    {
                        var cds = (ClassDeclarationSyntax)ctx.Node;
                        var symbol = ctx.SemanticModel.GetDeclaredSymbol(cds);

                        if (symbol is not INamedTypeSymbol classSymbol)
                            return null;

                        List<ITypeSymbol> contracts = [];

                        foreach(var iface in classSymbol.AllInterfaces)
                        {
                            if (!iface.IsGenericType || iface.Name!="IMessageTypeEncoder")
                                continue;
                            contracts.Add(iface.TypeArguments[0]);
                        }

                        if (contracts.Count>0)
                            return new ContractEncoder(classSymbol, contracts);

                        return null;
                    }
                )
                .Where(static s => s is not null)!;
    }
}
