using dotUnion.SourceGenerator.Extensions;
using dotUnion.SourceGenerator.Data;

namespace dotUnion.SourceGenerator.Model;

public sealed record UnionTarget(
	string Namespace,
	string Name,
	EquatableArray<UnionTargetMember> Members,
	EquatableArray<string> TypeParameters,
	EquatableArray<string> UsingDirectives,
	ParentType? ParentType
)
{
	public string GenericDeclaration
		=> TypeParameters.JoinString() is not { Length: > 0 } parameters ? "" : $"<{parameters}>";

	public string FullName => Name + GenericDeclaration;

	public string BaseUnion
		=> Members.Length > 0
			? $"Union<{FullName}, {Members.JoinSelect(m => $"{FullName}.{m.Name}")}>"
			: $"Union<{FullName}>";

	public string FullyQualifiedBaseUnion(string qualification)
		=> Members.Length > 0
			? $"Union<{qualification}{FullName}, {Members.JoinSelect(m => $"{qualification}{FullName}.{m.Name}")}>"
			: $"Union<{qualification}{FullName}>";
}
