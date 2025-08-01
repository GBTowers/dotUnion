using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace dotUnion.SourceGenerator.Data;

/// <summary>
///   An imutable, equatable array. This is equivalent to <see cref="ImmutableArray{T}" /> but with value equality support.
/// </summary>
/// <typeparam name="T">The type of values in the array.</typeparam>
[SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global"),
SuppressMessage(category: "ReSharper", checkId: "MemberCanBePrivate.Global")]
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
where T : IEquatable<T>?
{
	/// <summary>
	///   The underlying <typeparamref name="T" /> array.
	/// </summary>
	private readonly T[]? _array;

	public int Length => _array?.Length ?? 0;

	/// <summary>
	///   Creates a new <see cref="EquatableArray{T}" /> instance.
	/// </summary>
	/// <param name="array">The input <see cref="ImmutableArray{T}" /> to wrap.</param>
	public EquatableArray(ImmutableArray<T> array) => _array = Unsafe.As<ImmutableArray<T>, T[]?>(ref array);

	/// <summary>
	///   Gets a reference to an item at a specified position within the array.
	/// </summary>
	/// <param name="index">The index of the item to retrieve a reference to.</param>
	/// <returns>A reference to an item at a specified position within the array.</returns>
	public ref readonly T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref AsImmutableArray().ItemRef(index);
	}

	/// <summary>
	///   Gets a value indicating whether the current array is empty.
	/// </summary>
	public bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)] get => AsImmutableArray().IsEmpty;
	}

	/// <sinheritdoc />
	public bool Equals(EquatableArray<T> array) => AsImmutableArray().SequenceEqual(array.AsImmutableArray());

	/// <sinheritdoc />
	public override bool Equals([NotNullWhen(returnValue: true)] object? obj)
		=> obj is EquatableArray<T> array && Equals(objA: this, objB: array);

	/// <sinheritdoc />
	public override int GetHashCode()
	{
		if (_array is not {} array) return 0;

		HashCode hashCode = default;

		foreach (T item in array) hashCode.Add(item);

		return hashCode.ToHashCode();
	}

	/// <summary>
	///   Gets an <see cref="ImmutableArray{T}" /> instance from the current <see cref="EquatableArray{T}" />.
	/// </summary>
	/// <returns>The <see cref="ImmutableArray{T}" /> from the current <see cref="EquatableArray{T}" />.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ImmutableArray<T> AsImmutableArray() => Unsafe.As<T[]?, ImmutableArray<T>>(ref Unsafe.AsRef(in _array));

	/// <summary>
	///   Creates an <see cref="EquatableArray{T}" /> instance from a given <see cref="ImmutableArray{T}" />.
	/// </summary>
	/// <param name="array">The input <see cref="ImmutableArray{T}" /> instance.</param>
	/// <returns>An <see cref="EquatableArray{T}" /> instance from a given <see cref="ImmutableArray{T}" />.</returns>
	public static EquatableArray<T> FromImmutableArray(ImmutableArray<T> array) => new(array);

	/// <summary>
	///   Returns a <see cref="ReadOnlySpan{T}" /> wrapping the current items.
	/// </summary>
	/// <returns>A <see cref="ReadOnlySpan{T}" /> wrapping the current items.</returns>
	public ReadOnlySpan<T> AsSpan() => AsImmutableArray().AsSpan();

	/// <summary>
	///   Copies the contents of this <see cref="EquatableArray{T}" /> instance. to a mutable array.
	/// </summary>
	/// <returns>The newly instantiated array.</returns>
	public T[] ToArray() => AsImmutableArray().ToArray();

	/// <summary>
	///   Gets an <see cref="ImmutableArray{T}.Enumerator" /> value to traverse items in the current array.
	/// </summary>
	/// <returns>An <see cref="ImmutableArray{T}.Enumerator" /> value to traverse items in the current array.</returns>
	public ImmutableArray<T>.Enumerator GetEnumerator() => AsImmutableArray().GetEnumerator();

	/// <inheritdoc />
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)AsImmutableArray()).GetEnumerator();

	/// <summary>
	///   Implicitly converts an <see cref="ImmutableArray{T}" /> to <see cref="EquatableArray{T}" />.
	/// </summary>
	/// <returns>An <see cref="EquatableArray{T}" /> instance from a given <see cref="ImmutableArray{T}" />.</returns>
	public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => FromImmutableArray(array);

	/// <summary>
	///   Implicitly converts an <see cref="EquatableArray{T}" /> to <see cref="ImmutableArray{T}" />.
	/// </summary>
	/// <returns>An <see cref="ImmutableArray{T}" /> instance from a given <see cref="EquatableArray{T}" />.</returns>
	public static implicit operator ImmutableArray<T>(EquatableArray<T> array) => array.AsImmutableArray();

	/// <summary>
	///   Checks whether two <see cref="EquatableArray{T}" /> values are the same.
	/// </summary>
	/// <param name="left">The first <see cref="EquatableArray{T}" /> value.</param>
	/// <param name="right">The second <see cref="EquatableArray{T}" /> value.</param>
	/// <returns>Whether <paramref name="left" /> and <paramref name="right" /> are equal.</returns>
	public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

	/// <summary>
	///   Checks whether two <see cref="EquatableArray{T}" /> values are not the same.
	/// </summary>
	/// <param name="left">The first <see cref="EquatableArray{T}" /> value.</param>
	/// <param name="right">The second <see cref="EquatableArray{T}" /> value.</param>
	/// <returns>Whether <paramref name="left" /> and <paramref name="right" /> are not equal.</returns>
	public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}
