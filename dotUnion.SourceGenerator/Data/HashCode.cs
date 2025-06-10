using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

#pragma warning disable CS0809

namespace dotUnion.SourceGenerator.Data;

/// <summary>
///   A polyfill type that mirrors some methods from <see cref="HashCode" /> on .NET 6.
/// </summary>
internal struct HashCode
{
	private const uint Prime1 = 2654435761U;
	private const uint Prime2 = 2246822519U;
	private const uint Prime3 = 3266489917U;
	private const uint Prime4 = 668265263U;
	private const uint Prime5 = 374761393U;

	private static readonly uint Seed = GenerateGlobalSeed();

	private uint _v1, _v2, _v3, _v4;
	private uint _queue1, _queue2, _queue3;
	private uint _length;

	/// <summary>
	///   Initializes the default seed.
	/// </summary>
	/// <returns>A random seed.</returns>
	// ReSharper disable once RedundantUnsafeContext
	private static unsafe uint GenerateGlobalSeed()
	{
		var bytes = new byte[4];

		RandomNumberGenerator.Create().GetBytes(bytes);

		return BitConverter.ToUInt32(value: bytes, startIndex: 0);
	}

	/// <summary>
	///   Adds a single value to the current hash.
	/// </summary>
	/// <typeparam name="T">The type of the value to add into the hash code.</typeparam>
	/// <param name="value">The value to add into the hash code.</param>
	public void Add<T>(T value)
	{
		Add(value?.GetHashCode() ?? 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Initialize(
		out uint v1,
		out uint v2,
		out uint v3,
		out uint v4
	)
	{
		v1 = Seed + Prime1 + Prime2;
		v2 = Seed + Prime2;
		v3 = Seed;
		v4 = Seed - Prime1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Round(uint hash, uint input) => RotateLeft(value: hash + (input * Prime2), offset: 13) * Prime1;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint QueueRound(uint hash, uint queuedValue)
		=> RotateLeft(value: hash + (queuedValue * Prime3), offset: 17) * Prime4;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint MixState(
		uint v1,
		uint v2,
		uint v3,
		uint v4
	)
		=> RotateLeft(value: v1, offset: 1)
		+ RotateLeft(value: v2, offset: 7)
		+ RotateLeft(value: v3, offset: 12)
		+ RotateLeft(value: v4, offset: 18);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint MixEmptyState() => Seed + Prime5;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint MixFinal(uint hash)
	{
		hash ^= hash >> 15;
		hash *= Prime2;
		hash ^= hash >> 13;
		hash *= Prime3;
		hash ^= hash >> 16;

		return hash;
	}

	private void Add(int value)
	{
		var val = (uint)value;
		uint previousLength = _length++;
		uint position = previousLength % 4;

		switch (position)
		{
			case 0: _queue1 = val; break;
			case 1: _queue2 = val; break;
			case 2: _queue3 = val; break;
			default:
			{
				if (previousLength == 3)
					Initialize(
						v1: out _v1,
						v2: out _v2,
						v3: out _v3,
						v4: out _v4
					);

				_v1 = Round(hash: _v1, input: _queue1);
				_v2 = Round(hash: _v2, input: _queue2);
				_v3 = Round(hash: _v3, input: _queue3);
				_v4 = Round(hash: _v4, input: val);
				break;
			}
		}
	}

	/// <summary>
	///   Gets the resulting hashcode from the current instance.
	/// </summary>
	/// <returns>The resulting hashcode from the current instance.</returns>
	public int ToHashCode()
	{
		uint length = _length;
		uint position = length % 4;
		uint hash = length < 4
			? MixEmptyState()
			: MixState(
				v1: _v1,
				v2: _v2,
				v3: _v3,
				v4: _v4
			);

		hash += length * 4;

		if (position > 0)
		{
			hash = QueueRound(hash: hash, queuedValue: _queue1);

			if (position > 1)
			{
				hash = QueueRound(hash: hash, queuedValue: _queue2);

				if (position > 2) hash = QueueRound(hash: hash, queuedValue: _queue3);
			}
		}

		hash = MixFinal(hash);

		return (int)hash;
	}

	/// <inheritdoc />
	[Obsolete(
		message:
		"HashCode is a mutable struct and should not be compared with other HashCodes. Use ToHashCode to retrieve the computed hash code.",
		error: true
	), EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode() => throw new NotSupportedException();

	/// <inheritdoc />
	[Obsolete(message: "HashCode is a mutable struct and should not be compared with other HashCodes.", error: true),
	EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object? obj) => throw new NotSupportedException();

	/// <summary>
	///   Rotates the specified value left by the specified number of bits.
	///   Similar in behavior to the x86 instruction ROL.
	/// </summary>
	/// <param name="value">The value to rotate.</param>
	/// <param name="offset">
	///   The number of bits to rotate by.
	///   Any value outside the range [0..31] is treated as congruent mod 32.
	/// </param>
	/// <returns>The rotated value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> (32 - offset));
}
