using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using dotUnion.Attributes;
using dotUnion.SourceGenerator.Extensions;
using dotUnion.SourceGenerator.Composers;
using dotUnion.SourceGenerator.Data;
using dotUnion.SourceGenerator.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace dotUnion.SourceGenerator;

[Generator]
public sealed class UnionGenerator : IIncrementalGenerator
{
	private const string AsyncExtensionsOption = "build_property.AsyncUnionExtensions";
	private const string FullyQualifiedAttributeName = "dotUnion.Attributes.UnionAttribute";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<GeneratorOptions> options = context.AnalyzerConfigOptionsProvider.Select(ParseOptions)
		.WithComparer(EqualityComparer<GeneratorOptions>.Default);

		IncrementalValuesProvider<UnionTarget> pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
				fullyQualifiedMetadataName: typeof(UnionAttribute).FullName ?? FullyQualifiedAttributeName,
				predicate: SyntaxValidation,
				transform: SemanticTransform
			)
		.Where(u => u is not null)!;

		IncrementalValuesProvider<FakeUnion> unions = pipeline.Collect()
		.SelectMany((targets, _) => targets.FilterSelect(u => u.Members.Length > 0, u => u.Members.Length)
			.Distinct()
			.Select(arity => new FakeUnion(arity))
			);

		context.RegisterSourceOutput(source: unions.Combine(options), action: AritySourceComposer.BuildArity);

		context.RegisterSourceOutput(source: pipeline.Combine(options), action: UnionSourceComposer.BuildUnion);
	}

	private static GeneratorOptions ParseOptions(AnalyzerConfigOptionsProvider options, CancellationToken _)
		=> new(
			options.GlobalOptions.TryGetValue(key: AsyncExtensionsOption, value: out string? value) && IsFeatureEnabled(value)
		);

	private static bool IsFeatureEnabled(string property)
		=> StringComparer.OrdinalIgnoreCase.Equals(x: "enable", property)
		|| StringComparer.OrdinalIgnoreCase.Equals(x: "enabled", property)
		|| StringComparer.OrdinalIgnoreCase.Equals(x: "true", property);

	private static UnionTarget? SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetNode is not RecordDeclarationSyntax candidate) return null;
		if (!candidate.Modifiers.Any(SyntaxKind.PartialKeyword)) return null;
		if (candidate.Modifiers.Any(SyntaxKind.SealedKeyword)) return null;

		(ParentType? parentType, bool allParentsArePartial) = GetParentClasses(candidate);

		if (!allParentsArePartial) return null;

		return new UnionTarget(
			Namespace: context.TargetSymbol.ContainingNamespace.ToDisplayString(),
			Name: context.TargetSymbol.Name,
			Members: GetUnionTargetMembers(context, candidate),
			TypeParameters: ExtractTypeParameters(candidate),
			UsingDirectives: ExtractUsings(context.TargetNode),
			ParentType: parentType
		);
	}

	private static EquatableArray<UnionTargetMember> GetUnionTargetMembers(
		GeneratorAttributeSyntaxContext context,
		RecordDeclarationSyntax candidate
	)
	{
		List<UnionTargetMember> members = [];

		foreach (MemberDeclarationSyntax memberCandidate in candidate.Members)
		{
			// Union type can only have record members
			if (memberCandidate is not RecordDeclarationSyntax member) continue;

			// Union type can only have public members
			if (member.IsNonPublic()) continue;

			// Union type cannot have members with type parameters
			if (member.TypeParameterList?.Parameters.Count > 0) continue;

			// Union type cannot have non interface base
			if (member.HasNonInterfaceBaseType(context.SemanticModel)) continue;

			// Can only generate code for partial members
			if (!member.Modifiers.Any(SyntaxKind.PartialKeyword)) continue;

			RecordConstructor? constructor = GetMemberDefaultConstructor(member);

			members.Add(new UnionTargetMember(Name: member.Identifier.Text, Constructor: constructor));
		}

		return members.ToImmutableArray();
	}

	private static RecordConstructor? GetMemberDefaultConstructor(TypeDeclarationSyntax member)
	{
		if (member.ParameterList?.Parameters is not { Count: > 0 } parameterList) return null;

		EquatableArray<ConstructorParameter> parameters = parameterList
		.Select(p => new ConstructorParameter(Type: p.Type?.ToString() ?? string.Empty, Name: p.Identifier.Text))
		.ToImmutableArray();

		return new RecordConstructor(parameters);
	}

	private static (ParentType?, bool) GetParentClasses(SyntaxNode typeSyntax)
	{
		var parentSyntax = typeSyntax.Parent as TypeDeclarationSyntax;

		ParentType? parentInfo = null;

		var allParentsArePartial = true;

		while (parentSyntax != null && IsAllowedKind(parentSyntax.Kind()))
		{
			if (!parentSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)) allParentsArePartial = false;

			parentInfo = new ParentType(
				Keyword: parentSyntax.Keyword.ValueText,
				Name: parentSyntax.Identifier.ToString() + parentSyntax.TypeParameterList,
				Constraints: parentSyntax.ConstraintClauses.ToString(),
				Child: parentInfo
			);

			parentSyntax = parentSyntax.Parent as TypeDeclarationSyntax;
		}

		return (parentInfo, allParentsArePartial);

		static bool IsAllowedKind(SyntaxKind kind)
			=> kind is SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or SyntaxKind.RecordDeclaration;
	}

	[SuppressMessage(category: "ReSharper", checkId: "LoopCanBeConvertedToQuery")]
	private static ImmutableArray<string> ExtractUsings(SyntaxNode target)
	{
		SyntaxList<UsingDirectiveSyntax> allUsings = SyntaxFactory.List<UsingDirectiveSyntax>();
		foreach (SyntaxNode? ancestor in target.Ancestors(ascendOutOfTrivia: false))
			allUsings = ancestor switch
			{
				BaseNamespaceDeclarationSyntax namespaceDeclaration => allUsings.AddRange(namespaceDeclaration.Usings),
				CompilationUnitSyntax compilationUnit => allUsings.AddRange(compilationUnit.Usings),
				_ => allUsings
			};

		return [..allUsings.Select(u => u.ToString())];
	}

	private static ImmutableArray<string> ExtractTypeParameters(RecordDeclarationSyntax recordMember)
		=> recordMember.TypeParameterList?.Parameters.Select(p => p.Identifier.Text).ToImmutableArray() ?? [];

	private static bool SyntaxValidation(SyntaxNode node, CancellationToken token)
		=> node is RecordDeclarationSyntax candidate
		&& candidate.Modifiers.Any(SyntaxKind.PartialKeyword)
		&& !candidate.Modifiers.Any(SyntaxKind.SealedKeyword);
}
