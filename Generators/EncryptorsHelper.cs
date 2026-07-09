using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MQContract.Generators;

internal static class EncryptorsHelper
{
    public static IncrementalValuesProvider<ContractEncryptor?> LocateEncryptors(IncrementalGeneratorInitializationContext context)
        => context.SyntaxProvider
            .CreateSyntaxProvider<ContractEncryptor?>(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax cds && cds.BaseList is not null,
                transform: static (ctx, _) =>
                {
                    var cds = (ClassDeclarationSyntax)ctx.Node;
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(cds);

                    if (symbol is not INamedTypeSymbol classSymbol)
                        return null;

                    List<ITypeSymbol> contracts = [];

                    foreach (var iface in classSymbol.AllInterfaces)
                    {
                        if (!iface.IsGenericType || iface.Name!="IMessageTypeEncryptor")
                            continue;
                        contracts.Add(iface.TypeArguments[0]);
                    }

                    if (contracts.Count>0)
                        return new ContractEncryptor(classSymbol, contracts);

                    return null;
                }
            )
            .Where(static s => s is not null)!;
}
