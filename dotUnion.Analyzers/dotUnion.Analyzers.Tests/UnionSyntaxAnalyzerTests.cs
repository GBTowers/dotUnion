using System.Threading.Tasks;
using dotUnion.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace dotUnion.Analyzers.Tests;

public class UnionSyntaxAnalyzerTests
{
	[Fact]
	public async Task UL1001_UnionMissingPartialKeyword()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				[Union]
				public {|UL1001:record Result<T, TE>|}
				{
					partial record Ok(T Value);
					partial record Err(TE Error);
				}

				"""
		};

		await context.RunAsync();
	}

	[Fact]
	public async Task UL1002_ParentMissingPartialKeyword()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				public {|UL1002:class ParentClass|}
				{
					[Union]
					public partial record Result<T, TE>
					{
						partial record Ok(T Value);
						partial record Err(TE Error);
					}
				}
				"""
		};

		await context.RunAsync();
	}
	
	[Fact]
	public async Task UL1003_UnionCannotBeSealed()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				[Union]
				public sealed partial {|UL1003:record Result<T, TE>|}
				{
					partial record Ok(T Value);
					partial record Err(TE Error);
				}

				"""
		};

		await context.RunAsync();
	}
	
	[Fact]
	public async Task UL1004_UnionCannotHaveBaseType()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				public record BaseRecord;

				[Union]
				public partial {|UL1004:record Result<T, TE>|} : BaseRecord
				{
					partial record Ok(T Value);
					partial record Err(TE Error);
				}

				"""
		};

		await context.RunAsync();
	}
	
	[Fact]
	public async Task UL1005_UnionCannotHaveBaseType()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				[Union]
				public partial record Result<T, TE>
				{
					private string _errorCode;
					{|UL1005:public Result|}(string errorCode)
					{
						_errorCode = errorCode;
					}
				
					partial record Ok(T Value);
					partial record Err(TE Error);
				}

				"""
		};

		await context.RunAsync();
	}
	
	[Fact]
	public async Task UL1006_UnionCanOnlyHaveOnePart()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				{|UL1006:[Union]
				public partial record Result<T, TE>
				{

					partial record Ok(T Value);
					partial record Err(TE Error);
				}|}
				
				{|UL1006:public partial record Result<T, TE>
				{
					partial record OtherPart;
				}|}

				"""
		};

		await context.RunAsync();
	}
	
	[Fact]
	public async Task UL2001_UnionMemberMissingPartialKeyword()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				[Union]
				public partial record Result<T, TE>
				{
					partial record Ok(T Value);
					{|UL2001:record Err|}(TE Error);
				}

				"""
		};

		await context.RunAsync();
	}
	
	[Fact]
	public async Task UL2002_UnionMemberCannotBeGeneric()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				[Union]
				public partial record Result<T, TE>
				{
					partial record Ok(T Value);
					partial record Err(TE Error);
					partial {|UL2002:record Warning<TWarning>|}(TWarning warning);
				}

				"""
		};

		await context.RunAsync();
	}
	
	[Fact]
	public async Task UL2003_UnionMemberCannotHaveBaseType()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				public record BaseRecord;
				
				[Union]
				public partial record Result<T, TE>
				{
					partial {|UL2003:record Ok|}(T Value) : BaseRecord;
					partial record Err(TE Error);
				}

				"""
		};

		await context.RunAsync();
	}
	
	[Fact]
	public async Task UL2004_UnionMemberMustBePartialRecord()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				[Union]
				public partial record Result<T, TE>
				{
					public {|UL2004:class Ok|}(T Value);
					partial record Err(TE Error);
				}

				"""
		};

		await context.RunAsync();
	}
	
	[Fact]
	public async Task UL2005_UnionMemberMustBePublic()
	{
		CSharpAnalyzerTest<UnionGenerationSyntaxAnalyzer, DefaultVerifier> context = new()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
			TestState =
			{
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(typeof(UnionAttribute).Assembly.Location)
				}
			},
			TestCode = """
				using dotUnion.Attributes;

				namespace Test;

				[Union]
				public partial record Result<T, TE>
				{
					protected partial {|UL2005:record Ok|}(T Value);
					partial record Err(TE Error);
				}

				"""
		};

		await context.RunAsync();
	}
}
