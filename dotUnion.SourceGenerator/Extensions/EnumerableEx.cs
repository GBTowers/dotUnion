using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace dotUnion.SourceGenerator.Extensions;

[Serializable, ContractClass(typeof(EnumerableEx))]
public static class EnumerableEx
{
	[SuppressMessage(category: "ReSharper", checkId: "LoopCanBeConvertedToQuery")]
	public static IEnumerable<TOut> FilterSelect<TIn, TOut>(
		this IEnumerable<TIn> source,
		Func<TIn, bool> filter,
		Func<TIn, TOut> map
	)
	{
		foreach (TIn item in source)
			if (filter(item)) yield return map(item);
	}
	
	public static string JoinSelect<T>(
		this IEnumerable<T> source,
		Func<T, string> selector,
		string separator = ", "
	)
		=> source.Select(selector).JoinString(separator);

	public static string JoinString<T>(this IEnumerable<T> source, string separator = ", ")
		=> string.Join(separator: separator, values: source);

	/// <inheritdoc cref="List{T}.IndexOf(T)" />
	public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> selector)
	{
		var index = 0;
		foreach (T element in source)
		{
			if (selector(element)) return index;

			index++;
		}

		return -1;
	}
}
