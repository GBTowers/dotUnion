using dotUnion.SourceGenerator.Model;

namespace dotUnion.SourceGenerator.Extensions;

public static class KeyValuePairEx
{
	public static void Deconstruct(
		this KeyValuePair<string, UnionTargetMember> memberGroup,
		out string tuple,
		out UnionTargetMember targetMember
	)
	{
		tuple = memberGroup.Key;
		targetMember = memberGroup.Value;
	}
}
