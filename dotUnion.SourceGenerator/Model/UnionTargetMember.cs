using dotUnion.SourceGenerator.Extensions;
using dotUnion.SourceGenerator.Data;

namespace dotUnion.SourceGenerator.Model;

public sealed record UnionTargetMember(
	string Name,
	RecordConstructor? Constructor
)
{
	public string TupleConstructor
		=> Constructor is not null && Constructor.Parameters.Any()
			? Enumerable.Range(start: 1, count: Constructor.Parameters.Count()).JoinSelect(i => $"tuple.Item{i}")
			: "";

	public string VariableName => Name.FirstCharToLower()!;

}

public sealed record RecordConstructor(EquatableArray<ConstructorParameter> Parameters)
{
	public string TupleSignature => $"({Parameters.JoinSelect(p => p.Type)})";
	public string ParametersSignature => $"({Parameters.JoinSelect(p => p.Name)})";
	public override string ToString() => $"({Parameters.JoinString()})";
}

public sealed record ConstructorParameter(string Type, string Name)
{
	public override string ToString() => $"{Type} {Name}";
}
