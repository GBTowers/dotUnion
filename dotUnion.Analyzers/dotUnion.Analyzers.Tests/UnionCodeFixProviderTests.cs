using System.Threading.Tasks;
using dotUnion.Attributes;
using dotUnion.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace dotUnion.Analyzers.Tests;

public class MissingPartialKeywordCodeFixProviderTests
{
	[Fact]
	public async Task UL1001_AddPartialKeyword()
	{
		const string text = """
			using dotUnion.Attributes;

			namespace Test;

			[Union]
			public {|UL1001:record Result<T, TE>|}
			{
				partial record Ok(T Value);
				partial record Err(TE Error);
			}

			""";

		const string newText = """
			using dotUnion.Attributes;

			namespace Test;

			[Union]
			public partial record Result<T, TE>
			{
				partial record Ok(T Value);
				partial record Err(TE Error);
			}

			""";

		var context = new CSharpCodeFixTest<UnionGenerationSyntaxAnalyzer, MissingPartialKeywordCodeFixProvider, DefaultVerifier>
		{
			TestCode = text,
			FixedCode = newText,
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
		};

		await context.RunAsync();
	}
}
