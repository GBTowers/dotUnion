using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace dotUnion.Analyzers.Extensions;

[SuppressMessage(category: "ReSharper", checkId: "MemberCanBePrivate.Global")]
internal static class SymbolExtensions
{
	public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
	{
		ITypeSymbol? current = type;
		while (current is not null)
		{
			yield return current;

			current = current.BaseType;
		}
	}

	[SuppressMessage(category: "ReSharper", checkId: "InvertIf")]
	private static IList<INamedTypeSymbol> GetAllInterfacesIncludingThis(this ITypeSymbol type)
	{
		ImmutableArray<INamedTypeSymbol> allInterfaces = type.AllInterfaces;
		if (type is INamedTypeSymbol { TypeKind: TypeKind.Interface } namedType && !allInterfaces.Contains(namedType))
		{
			var result = new List<INamedTypeSymbol>(allInterfaces.Length + 1) { namedType };
			result.AddRange(allInterfaces);
			return result;
		}

		return allInterfaces;
	}

	public static bool DerivesFromType(this ITypeSymbol symbol, ITypeSymbol otherType)
	{
		IEnumerable<ITypeSymbol> baseTypes = GetBaseTypesAndThis(symbol);
		IList<INamedTypeSymbol> implementedInterfaces = GetAllInterfacesIncludingThis(symbol);

		return baseTypes.Any(baseType => SymbolEqualityComparer.Default.Equals(baseType, otherType))
		|| implementedInterfaces.Any(baseInterfaceType => SymbolEqualityComparer.Default.Equals(
					baseInterfaceType,
					otherType
				)
			);
	}

	public static SyntaxReference[] GetNonGeneratedParts(this ISymbol symbol, CancellationToken cancellationToken)
		=> symbol.DeclaringSyntaxReferences.Where(reference => !reference.SyntaxTree.IsGeneratedCode(cancellationToken))
			.ToArray();
}
