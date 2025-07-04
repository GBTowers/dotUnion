using dotUnion.Attributes;

namespace dotUnion.Integration;

[Union]
public partial record Result<T, TE>
{
	partial record Ok(T Value);
	partial record Err(TE Error);
}

public class UnionIntegration
{
	[Fact]
	public async Task MatchMethodWorksOnDerivedClasses()
	{
		Result<int, string> result = 4;

		string s = result.Match(f1: success => success.Value.ToString(), f2: failure => failure.Error);

		Assert.Equal(expected: "4", actual: s);


		Task<Result<int, string>> resultTask = Task.FromResult(result);

		_ = await resultTask.Union().Match(ok => ok.Value.ToString(), err => err.Error);
	}
}
