using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using dotUnion.Analyzers.Extensions;
using dotUnion.Attributes;
using dotUnion.Analyzers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace dotUnion.Analyzers;

/// <summary>
///   A simple analyzer for the Union Source Generator
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnionGenerationSyntaxAnalyzer : DiagnosticAnalyzer
{
#region TypeRules

	public static readonly DiagnosticDescriptor UL1001 = new(
		id: nameof(UL1001),
		title: "Union target must be partial",
		messageFormat: "Type '{0}' marked for source generation is missing partial keyword",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: """
		Type marked with union attribute must be partial 
		for the union source generator to work.
		"""
	);

	public static readonly DiagnosticDescriptor UL1002 = new(
		id: nameof(UL1002),
		title: "Union parents must be partial",
		messageFormat: "Type '{0}' must be partial, as it contains type '{1}'",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "Union parents must be marked as partial for the source generator to work."
	);


	public static readonly DiagnosticDescriptor UL1003 = new(
		id: nameof(UL1003),
		title: "Union type cannot be sealed",
		messageFormat: "Union Type '{0}' cannot be sealed",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: """
		Union type needs to be derived by its children for the generated code to work,
		so the marked type cannot be sealed.
		"""
	);

	public static readonly DiagnosticDescriptor UL1004 = new(
		id: nameof(UL1004),
		title: "Union target cannot have base type",
		messageFormat: "Record type '{0}' marked for generation cannot have non-interface base type or it will be ignored",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "Source generator will ignore types with base types."
	);

	public static readonly DiagnosticDescriptor UL1005 = new(
		id: nameof(UL1005),
		title: "Union parent cannot have non-private constructor",
		messageFormat: "'{0}' type can only have private constructors",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "Union Types can only have private constructors.",
		customTags: [WellKnownDiagnosticTags.NotConfigurable]
	);

	public static readonly DiagnosticDescriptor UL1006 = new(
		id: nameof(UL1006),
		title: "Union type can only have one non-generated part",
		messageFormat: "'{0}' is a union and can only have one non-generated part",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "Unions can only have one non-generated part.",
		customTags: [WellKnownDiagnosticTags.NotConfigurable]
	);

#endregion


#region MemberRules

	public static readonly DiagnosticDescriptor UL2001 = new(
		id: nameof(UL2001),
		title: "Union member must be partial",
		messageFormat: "Type '{0}' must be partial to be considered part of the union",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "Union parts must be partial to be marked as part of the union."
	);

	public static readonly DiagnosticDescriptor UL2002 = new(
		id: nameof(UL2002),
		title: "Union member cannot be generic",
		messageFormat: "'{0}' has generic parameters. Union members cannot be generic.",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "Union members cannot be generic, generic parameters should be on the parent."
	);

	public static readonly DiagnosticDescriptor UL2003 = new(
		id: nameof(UL2003),
		title: "Union member cannot have base type",
		messageFormat: "Union members cannot have base type, '0' has a non-interface base type",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "Source generator will ignore unions with members with base types."
	);


	public static readonly DiagnosticDescriptor UL2004 = new(
		id: nameof(UL2004),
		title: "Union type member must be a record and partial",
		messageFormat: "'{0}' Is not a partial record and will not be considered as part of the union",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "Unions can only have union members as type members."
	);

	public static readonly DiagnosticDescriptor UL2005 = new(
		id: nameof(UL2005),
		title: "Union type member must be public",
		messageFormat: "'{0}' contains a non-public access modifier keyword, union members must be public",
		category: DiagnosticCategory.Union,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "Unions can only have public type members."
	);

#endregion

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
	[
		UL1001,
		UL1002,
		UL1003,
		UL1004,
		UL1005,
		UL2001,
		UL2002,
		UL2003,
		UL2004,
		UL2005,
		UL1006
	];

	private const string FullyQualifiedMarkerAttributeName = "dotUnion.Attributes.UnionAttribute";

	public override void Initialize(AnalysisContext analysisContext)
	{
		analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		analysisContext.EnableConcurrentExecution();

		analysisContext.RegisterCompilationStartAction(context =>
			{
				INamedTypeSymbol? markerAttributeSymbol = context.Compilation.GetTypeByMetadataName(
					typeof(UnionAttribute).FullName ?? FullyQualifiedMarkerAttributeName
				);

				if (markerAttributeSymbol is null) return;


				context.RegisterSyntaxNodeAction(
					ctx => AnalyzeSyntax(ctx, markerAttributeSymbol),
					SyntaxKind.RecordDeclaration
				);
			}
		);
	}

	private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context, INamedTypeSymbol markerAttributeSymbol)
	{
		if (context.Node is not RecordDeclarationSyntax recordDeclaration) return;

		string attributeName = nameof(UnionAttribute).Replace(nameof(Attribute), string.Empty);

		if (!recordDeclaration.ContainsAttributeWithName(attributeName)) return;
		if (context.ContainingSymbol?.ContainsMarkerAttribute(markerAttributeSymbol) is not true) return;


		if (context.ContainingSymbol?.GetNonGeneratedParts(context.CancellationToken) is { Length: > 1 } references)
			foreach (SyntaxReference reference in references)
				context.ReportDiagnostic(
					Diagnostic.Create(
						descriptor: UL1006,
						location: Location.Create(reference.SyntaxTree, reference.Span),
						messageArgs: recordDeclaration.Identifier.Text
					)
				);

		if (!recordDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
			context.ReportDiagnostic(
				Diagnostic.Create(
					descriptor: UL1001,
					recordDeclaration.GetFullIdentifierLocation(),
					messageArgs: recordDeclaration.Identifier.Text
				)
			);

		if (recordDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword))
			context.ReportDiagnostic(
				Diagnostic.Create(
					descriptor: UL1003,
					recordDeclaration.GetFullIdentifierLocation(),
					messageArgs: recordDeclaration.Identifier.Text
				)
			);

		if (recordDeclaration.ParameterList is not null)
			context.ReportDiagnostic(
				Diagnostic.Create(
					descriptor: UL1005,
					location: recordDeclaration.GetFullIdentifierLocation(),
					messageArgs: recordDeclaration.Identifier.Text
				)
			);

		if (recordDeclaration.HasNonInterfaceBaseType(context.SemanticModel))
			context.ReportDiagnostic(
				Diagnostic.Create(
					descriptor: UL1004,
					recordDeclaration.GetFullIdentifierLocation(),
					messageArgs: recordDeclaration.Identifier.Text
				)
			);


		var parentSyntax = recordDeclaration.Parent as TypeDeclarationSyntax;
		while (parentSyntax != null && IsPossibleTypeParent(parentSyntax.Kind()))
		{
			if (!parentSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
				context.ReportDiagnostic(
					Diagnostic.Create(
						descriptor: UL1002,
						location: parentSyntax.GetFullIdentifierLocation(),
						messageArgs: [parentSyntax.Identifier.Text, recordDeclaration.Identifier.Text]
					)
				);

			parentSyntax = parentSyntax.Parent as TypeDeclarationSyntax;
		}

		foreach (MemberDeclarationSyntax memberDeclaration in recordDeclaration.Members)
		{
			if (memberDeclaration is ConstructorDeclarationSyntax constructor)
			{
				if (!constructor.Modifiers.Any(SyntaxKind.PrivateKeyword))
					context.ReportDiagnostic(
						Diagnostic.Create(
							descriptor: UL1005,
							location: Location.Create(
								constructor.SyntaxTree,
								TextSpan.FromBounds(constructor.SpanStart, constructor.Identifier.Span.End)
							),
							messageArgs: recordDeclaration.Identifier.Text
						)
					);

				continue;
			}

			if (memberDeclaration is not TypeDeclarationSyntax member) continue;

			if (member.Kind() is not SyntaxKind.RecordDeclaration)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						descriptor: UL2004,
						location: member.GetFullIdentifierLocation(),
						messageArgs: member.Identifier.Text
					)
				);

				continue;
			}

			if (member.TypeParameterList?.Parameters.Count > 0)
				context.ReportDiagnostic(
					Diagnostic.Create(
						descriptor: UL2002,
						location: member.GetFullIdentifierLocation(),
						messageArgs: member.Identifier.Text
					)
				);

			if (member.IsNonPublic())
				context.ReportDiagnostic(
					Diagnostic.Create(
						descriptor: UL2005,
						location: member.GetFullIdentifierLocation(),
						messageArgs: member.Identifier.Text
					)
				);

			if (member.HasNonInterfaceBaseType(context.SemanticModel))
				context.ReportDiagnostic(
					Diagnostic.Create(
						descriptor: UL2003,
						location: member.GetFullIdentifierLocation(),
						messageArgs: member.Identifier.Text
					)
				);

			if (!member.Modifiers.Any(SyntaxKind.PartialKeyword))
				context.ReportDiagnostic(
					Diagnostic.Create(
						descriptor: UL2001,
						location: member.GetFullIdentifierLocation(),
						messageArgs: member.Identifier.Text
					)
				);
		}
	}

	private static bool IsPossibleTypeParent(SyntaxKind kind)
		=> kind is SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or SyntaxKind.RecordDeclaration;
}

public static class BaseTypeDeclarationSyntaxEx
{
	public static bool ContainsMarkerAttribute(this ISymbol symbol, INamedTypeSymbol markerAttribute)
		=> symbol.GetAttributes()
		.Any(attributeData => markerAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default));

	[SuppressMessage(category: "ReSharper", checkId: "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
	public static bool ContainsAttributeWithName(this BaseTypeDeclarationSyntax type, string attributeName)
	{
		foreach (AttributeListSyntax attributeList in type.AttributeLists)
			foreach (AttributeSyntax attribute in attributeList.Attributes)
				if (attribute.Name.ToString() == attributeName)
					return true;

		return false;
	}

	public static bool HasNonInterfaceBaseType(this BaseTypeDeclarationSyntax type, SemanticModel semanticModel)
		=> type.BaseList?.Types is { Count: > 0 } baseList
		&& semanticModel.GetSymbolInfo(baseList.First().Type).Symbol is not ITypeSymbol { TypeKind: TypeKind.Interface };

	public static Location GetFullIdentifierLocation(this TypeDeclarationSyntax declaration)
		=> Location.Create(
			declaration.SyntaxTree,
			TextSpan.FromBounds(
				declaration.Keyword.SpanStart,
				declaration.TypeParameterList?.Span.End ?? declaration.Identifier.Span.End
			)
		);
}
