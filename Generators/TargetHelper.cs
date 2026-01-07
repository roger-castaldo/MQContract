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
                    var contractType = a.ConstructorArguments[0].Value as ITypeSymbol;
                    var encoders = (IEnumerable<INamedTypeSymbol>?)(a.ConstructorArguments[1].IsNull ? null : [(INamedTypeSymbol)a.ConstructorArguments[1].Value!]);
                    var converters = (IEnumerable<ITypeSymbol>?)(a.ConstructorArguments[2].IsNull ? null : a.ConstructorArguments[2].Values.Select(v=>(ITypeSymbol)v.Value!));
                    var encryptors = (IEnumerable<INamedTypeSymbol>?)(a.ConstructorArguments[3].IsNull ? null : [(INamedTypeSymbol)a.ConstructorArguments[3].Value!]);
                    return new ContractType(contractType!, encoders, converters, encryptors);
                })
                .ToImmutableArray()!;

            if (contracts.Length == 0)
                return null;

            var settingsAtt = symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name=="MQContractMessageContextAttribute");

            var settings = new ContextSettings(
                (settingsAtt?.ConstructorArguments.Length>=1 ? (bool?)settingsAtt?.ConstructorArguments[0].Value : null),
                (settingsAtt?.ConstructorArguments.Length>=2 ? (bool?)settingsAtt?.ConstructorArguments[1].Value : null),
                (settingsAtt?.ConstructorArguments.Length>=3 ? (bool?)settingsAtt?.ConstructorArguments[2].Value : null)
                );

            return new ContractContext(namedType, settings, contracts);
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
