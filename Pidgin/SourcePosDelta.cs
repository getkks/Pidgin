using System;

namespace Pidgin;

/// <summary>
/// Represents a difference in textual lines and columns corresponding to a region of an input stream.
/// </summary>
public readonly struct SourcePosDelta : IEquatable<SourcePosDelta>, IComparable<SourcePosDelta> {

	/// <summary>
	/// Create a new <see cref="SourcePosDelta"/> with the specified number of lines and columns.
	/// </summary>
	/// <param name="lines">The number of lines</param>
	/// <param name="cols">The number of columns</param>
	public SourcePosDelta( int lines, int cols ) {
		Lines = lines;
		Cols = cols;
	}

	/// <summary>
	/// A <see cref="SourcePosDelta"/> representing a newline being consumed.
	/// </summary>
	/// <returns>A <see cref="SourcePosDelta"/> representing a newline being consumed.</returns>
	public static SourcePosDelta NewLine { get; } = new SourcePosDelta(1, 0);

	/// <summary>
	/// A <see cref="SourcePosDelta"/> representing a single column being consumed.
	/// </summary>
	/// <returns>A <see cref="SourcePosDelta"/> representing a single column being consumed.</returns>
	public static SourcePosDelta OneCol { get; } = new SourcePosDelta(0, 1);

	/// <summary>
	/// A <see cref="SourcePosDelta"/> representing no change in the source position.
	/// </summary>
	/// <returns>A <see cref="SourcePosDelta"/> representing no change in the source position.</returns>
	public static SourcePosDelta Zero { get; } = new SourcePosDelta(0, 0);

	/// <summary>
	/// Gets the number of columns represented by the <see cref="SourcePosDelta"/>.
	/// </summary>
	/// <returns>The number of columns</returns>
	public int Cols {
		get;
	}

	/// <summary>
	/// Gets the number of lines represented by the <see cref="SourcePosDelta"/>.
	/// </summary>
	/// <returns>The number of lines</returns>
	public int Lines {
		get;
	}

	/// <inheritdoc/>
	public static bool operator !=( SourcePosDelta left, SourcePosDelta right )
		=> !left.Equals(right);

	/// <summary>
	/// Add two <see cref="SourcePosDelta"/>s.
	/// </summary>
	/// <param name="left">The first <see cref="SourcePosDelta"/>.</param>
	/// <param name="right">The <see cref="SourcePosDelta"/> to add to <paramref name="left"/>.</param>
	/// <returns>A <see cref="SourcePosDelta"/> representing the composition of <paramref name="left"/> and <paramref name="right"/>.</returns>
	public static SourcePosDelta operator +( SourcePosDelta left, SourcePosDelta right )
		=> left.Plus(right);

	/// <inheritdoc/>
	public static bool operator <( SourcePosDelta left, SourcePosDelta right )
		=> left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator <=( SourcePosDelta left, SourcePosDelta right )
		=> left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	public static bool operator ==( SourcePosDelta left, SourcePosDelta right )
		=> left.Equals(right);

	/// <inheritdoc/>
	public static bool operator >( SourcePosDelta left, SourcePosDelta right )
		=> left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator >=( SourcePosDelta left, SourcePosDelta right )
		=> left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public int CompareTo( SourcePosDelta other ) {
		var lineCmp = Lines.CompareTo(other.Lines);
		return lineCmp is 0 ? Cols.CompareTo(other.Cols) : lineCmp;
	}

	/// <inheritdoc/>
	public bool Equals( SourcePosDelta other )
		=> Lines == other.Lines
		&& Cols == other.Cols;

	/// <inheritdoc/>
	public override bool Equals( object? other )
		=> other is SourcePosDelta delta
		&& Equals(delta);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Lines, Cols);

	/// <summary>
	/// Add two <see cref="SourcePosDelta"/>s.
	/// </summary>
	/// <param name="other">The <see cref="SourcePosDelta"/> to add to this one.</param>
	/// <returns>A <see cref="SourcePosDelta"/> representing the composition of this and <paramref name="other"/>.</returns>
	public SourcePosDelta Plus( SourcePosDelta other )
		=> new(
			Lines + other.Lines,
			( other.Lines == 0 ? Cols : 0 ) + other.Cols
		);
}
