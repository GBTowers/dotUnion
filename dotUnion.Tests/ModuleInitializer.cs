using System.Runtime.CompilerServices;

namespace dotUnion.Tests;

public static class ModuleInitializer
{
	[ModuleInitializer]
	public static void Initialize()
	{
		VerifySourceGenerators.Initialize();
		VerifyDiffPlex.Initialize();
	}
}
