using dotUnion.SourceGenerator.Extensions;
using dotUnion.SourceGenerator.Data;

namespace dotUnion.SourceGenerator.Model;

public sealed record UnionTarget(
	string Namespace,
	string Name,
	EquatableArray<UnionTargetMember> Members,
	EquatableArray<string> TypeParameters,
	bool GenerateAsyncExtensions,
	EquatableArray<string> UsingDirectives,
	ParentType? ParentType
)
{
	public string GenericDeclaration
		=> TypeParameters.JoinString() is not { Length: > 0 } parameters ? "" : $"<{parameters}>";

	public string FullName => Name + GenericDeclaration;
}
