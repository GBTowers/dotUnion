namespace dotUnion.SourceGenerator.Extensions;

public static class EnumE
{
	public static T? ToEnum<T>(this string? str)
	where T : struct, Enum
		=> Enum.TryParse(value: str, result: out T output) ? output : default;
}
