using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotUnion.SourceGenerator.Extensions;

public static class SyntaxKindEx
{
	private static bool IsAccessModifier(this SyntaxKind kind)
		=> kind is SyntaxKind.PublicKeyword
		or SyntaxKind.PrivateKeyword
		or SyntaxKind.ProtectedKeyword
		or SyntaxKind.InternalKeyword
		or SyntaxKind.FileKeyword;
	
	public static bool HasNonInterfaceBaseType(this BaseTypeDeclarationSyntax type, SemanticModel semanticModel)
		=> type.BaseList?.Types is { Count: > 0 } baseList
		&& semanticModel.GetSymbolInfo(baseList.First().Type).Symbol is not ITypeSymbol { TypeKind: TypeKind.Interface };


	public static bool IsNonPublic(this BaseTypeDeclarationSyntax type) => type.Modifiers.Any(IsNonPublicAccessibility);

	private static bool IsNonPublicAccessibility(this SyntaxToken token)
		=> token.Kind() is SyntaxKind.PrivateKeyword
		or SyntaxKind.InternalKeyword
		or SyntaxKind.ProtectedKeyword
		or SyntaxKind.FileKeyword;
}
