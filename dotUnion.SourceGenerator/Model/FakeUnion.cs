using dotUnion.SourceGenerator.Extensions;

namespace dotUnion.SourceGenerator.Model;

public record FakeUnion(int Arity)
{
	public ArityMember[] ArityMembers { get; } = Arity <= 0
		? []
		: Enumerable.Range(start: 1, count: Arity).Select(i => new ArityMember(i)).ToArray();

	private const char BaseTypeParameter = 'T';
	private string TypeParameters => ArityMembers.Select(m => m.Name).Prepend(BaseTypeParameter.ToString()).JoinString();
	public string Declaration => "Union" + '<' + TypeParameters + '>';
	public string MatchTypeParams => "TOut, " + TypeParameters;

	public string WhereClauses
		=> $"where T : {Declaration} " + ArityMembers.JoinSelect(m => $"where {m.Name} : T", separator: " ");
}

public record ArityMember(int Arity)
{
	public string Name => "T" + Arity;
	public string VariableName => "t" + Arity;
	public string FuncName => "f" + Arity;
	public string FuncDeclaration => $"Func<{Name}, TOut>";
	public string AsyncFuncDeclaration => $"Func<{Name}, Task<TOut>>";
	public string AsyncValueFuncDeclaration => $"Func<{Name}, ValueTask<TOut>>";
	public string ActName => "a" + Arity;
	public string ActDeclaration => $"Action<{Name}>";
	public string AsyncActDeclaration => $"Func<{Name}, Task>";
	public string AsyncValueActDeclaration => $"Func<{Name}, ValueTask>";
}
