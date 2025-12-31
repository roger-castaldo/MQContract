using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MQContract.Generators
{
    internal static class TargetHelper
    {
        private static bool InheritsFrom(INamedTypeSymbol type, string baseName)
        {
            for (var current = type.BaseType; current != null; current = current.BaseType)
            {
                if (current.Name == baseName)
                    return true;
            }
            return false;
        }
        private static ContractContext? GetSemanticTarget(GeneratorSyntaxContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl);

            if (symbol is not INamedTypeSymbol namedType)
                return null;

            // Must inherit from MQContractMessageContext
            if (!InheritsFrom(namedType, "MQContractMessageContext"))
                return null;

            var contracts = namedType.GetAttributes()
                .Where(a => a.AttributeClass?.Name == "UseMqContractAttribute")
                .Select(a =>
                {
                    var contractType = a.ConstructorArguments[0].Value as INamedTypeSymbol;
                    var encoders = (IEnumerable<INamedTypeSymbol>?)(a.ConstructorArguments[1].IsNull ? null : [(INamedTypeSymbol)a.ConstructorArguments[1].Value]);
                    var converers = (IEnumerable<INamedTypeSymbol>?)(a.ConstructorArguments[2].IsNull ? null : [(INamedTypeSymbol)a.ConstructorArguments[2].Value]);
                    return new ContractType(contractType!, encoders, converers);
                })
                .ToImmutableArray()!;

            if (contracts.Length == 0)
                return null;

            return new ContractContext(namedType, contracts);
        }

        public static IncrementalValuesProvider<ContractContext?> LocateContexts(IncrementalGeneratorInitializationContext context)
            => context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) =>
                        node is ClassDeclarationSyntax cds &&
                        cds.AttributeLists.Count > 0 &&
                        cds.Modifiers.Any(SyntaxKind.PartialKeyword),
                    transform: static (ctx, _) => GetSemanticTarget(ctx))
                .Where(m => m.HasValue);
    }
}
