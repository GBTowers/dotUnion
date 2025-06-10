namespace dotUnion.Tests;

public class UnionGeneratorTests
{
	[Fact]
	public Task GeneratesPartialClassWhenAttributeIsPresent()
	{
		const string code = """
			using System;
			using dotUnion.Attributes;

			namespace Tests;

			[Union]
			public partial record ApiResult
			{
			    partial record Ok(string Message);
			    partial record BadRequest(string Message);
			}

			""";

		return UnionGeneratorTester.Verify(code);
	}

	[Fact]
	public Task HandlesBaseClassGenericParameters()
	{
		const string code = """
			using System;
			using dotUnion.Attributes;

			namespace Tests;

			[Union]
			public partial record Result<T, TE>
			{
			    partial record Ok(T Value);
			    partial record Err(TE Error);
			}


			""";

		return UnionGeneratorTester.Verify(code);
	}

	[Fact]
	public Task HandlesParentClass()
	{
		const string code = """
			using System;
			using dotUnion.Attributes;

			namespace Tests;

			public partial class Hello
			{
				[Union]
				public partial record Option<T>
				{
					partial record Some(T Value);
					partial record None;
				}
			}
			""";

		return UnionGeneratorTester.Verify(code);
	}
}

public class VerifyChecksTests
{
	[Fact]
	public Task Run() => VerifyChecks.Run();
}
