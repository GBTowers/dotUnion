using dotUnion.SourceGenerator.Data;

namespace dotUnion.SourceGenerator.Extensions;

public static class StringEx
{
	[return: NotNullIfNotNull(nameof(str))]
	public static string? FirstCharToLower(this string? str)
		=> str is { Length: > 0 } ? char.ToLower(str[index: 0]) + new string(str.Skip(count: 1).ToArray()) : null;

	public static bool IsNullOrWhiteSpace(this string? str) => string.IsNullOrWhiteSpace(str);
}
