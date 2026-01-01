using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MQContract.Generators
{
    internal readonly struct ContractType(INamedTypeSymbol contract, IEnumerable<INamedTypeSymbol>? encoders, IEnumerable<ITypeSymbol>? converters)
    {
        public INamedTypeSymbol Contract => contract;
        public IEnumerable<INamedTypeSymbol>? Encoders => encoders;
        public IEnumerable<ITypeSymbol>? Converters => converters;
    }

    internal readonly struct ContractEncoder(INamedTypeSymbol encoder, IEnumerable<ITypeSymbol> contracts)
    {
        public INamedTypeSymbol Encoder => encoder;
        public IEnumerable<ITypeSymbol> Contracts => contracts;
    }

    internal readonly struct ContractConverter(INamedTypeSymbol converter, IEnumerable<(ITypeSymbol from, ITypeSymbol to)> contracts)
    {
        public INamedTypeSymbol Converter => converter;
        public IEnumerable<(ITypeSymbol from, ITypeSymbol to)> Contracts => contracts;
    }

    internal readonly struct ContractContext(INamedTypeSymbol target, ImmutableArray<ContractType> contracts)
    {
        public INamedTypeSymbol Target => target;
        public ImmutableArray<ContractType> Contracts => contracts;
    }

    internal readonly struct ContractConverterPair(INamedTypeSymbol converter, ITypeSymbol from, ITypeSymbol to)
    {
        public INamedTypeSymbol Converter => converter;
        public ITypeSymbol From => from;
        public ITypeSymbol To => to;
    }
}
