using System;
using System.Collections.Immutable;
using System.Text;

using LExpression = System.Linq.Expressions.Expression;

namespace Pidgin;

/// <summary>
/// Represents a parsing expectation for error reporting.
/// Expected values are either a sequence of expected tokens (in which case <c>Label == null &amp;&amp; Tokens != null</c>),
/// a custom-named parser (<c>Label != null &amp;&amp; Tokens == null</c>),
/// or the end of the input stream (<c>Label == null &amp;&amp; Tokens == null</c>)
/// </summary>
public readonly struct Expected<TToken> : IEquatable<Expected<TToken>>, IComparable<Expected<TToken>> {
	private static readonly bool IsChar = typeof(TToken).Equals(typeof(char));

	private static Func<TToken, char>? _castToChar;

	internal Expected( string label ) {
		Label = label;
		Tokens = default;
	}

	internal Expected( ImmutableArray<TToken> tokens ) {
		Label = null;
		Tokens = tokens;
	}

	/// <summary>
	/// Did the parser expect the end of the input stream?
	/// </summary>
	/// <returns>True if the parser expected the end of the input stream</returns>
	public bool IsEof => Label == null && Tokens == null;

	/// <summary>
	/// The custom name of the parser that produced this error, or null if the expectation was a sequence of tokens.
	/// </summary>
	/// <returns>The label</returns>
	public string? Label {
		get;
	}

	/// <summary>
	/// The sequence of tokens that were expected at the point of the error, null if the parser had a custom name.
	/// </summary>
	/// <returns>The sequence of tokens that were expected</returns>
	public ImmutableArray<TToken> Tokens {
		get;
	}

	private static Func<TToken, char> CastToChar {
		get {
			if( _castToChar == null ) {
				var param = LExpression.Parameter(typeof(TToken));
				_castToChar = LExpression.Lambda<Func<TToken, char>>(param, param).Compile();
			}
			return _castToChar;
		}
	}

	/// <inheritdoc/>
	public static bool operator !=( Expected<TToken> left, Expected<TToken> right )
		=> !left.Equals(right);

	/// <inheritdoc/>
	public static bool operator <( Expected<TToken> left, Expected<TToken> right )
		=> left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator <=( Expected<TToken> left, Expected<TToken> right )
		=> left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	public static bool operator ==( Expected<TToken> left, Expected<TToken> right )
		=> left.Equals(right);

	/// <inheritdoc/>
	public static bool operator >( Expected<TToken> left, Expected<TToken> right )
		=> left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator >=( Expected<TToken> left, Expected<TToken> right )
		=> left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public int CompareTo( Expected<TToken> other ) =>
		// Label < Tokens < EOF
		Label != null
			? other.Label != null ? string.Compare(Label, other.Label) : -1
			: !Tokens.IsDefault
			? other.Label != null ? 1 : !other.Tokens.IsDefault ? EnumerableExtensions.Compare(Tokens, other.Tokens) : -1
			: other.Label == null && other.Tokens == null ? 0 : 1;

	/// <inheritdoc/>
	public bool Equals( Expected<TToken> other )
		=> object.Equals(Label, other.Label)
		&& ( Tokens.IsDefault && Tokens.IsDefault
			|| !Tokens.IsDefault && !Tokens.IsDefault && EnumerableExtensions.Equal(Tokens, other.Tokens)
		);

	/// <inheritdoc/>
	public override bool Equals( object? other )
		=> other is not null
		&& other is Expected<TToken> expected
		&& Equals(expected);

	/// <inheritdoc/>
	public override int GetHashCode() {
		unchecked {
			var hash = 17;
			hash = hash * 23 + Label?.GetHashCode() ?? 0;
			hash = hash * 23 + EnumerableExtensions.GetHashCode(Tokens);
			return hash;
		}
	}

	/// <inheritdoc/>
	public override string ToString() {
		if( IsEof ) {
			return "EOF";
		}
		var sb = new StringBuilder();
		_ = sb.Append(Label != null ? "Label: " : "Tokens: ");
		AppendTo(sb);
		return sb.ToString();
	}

	internal void AppendTo( StringBuilder sb ) {
		if( IsEof ) {
			_ = sb.Append("end of input");
			return;
		}
		if( Label != null ) {
			_ = sb.Append(Label);
			return;
		}

		var tokens = Tokens!;
		_ = sb.Append('"');
		if( IsChar ) {
			foreach( var token in tokens ) {
				var chr = CastToChar(token);
				_ = sb.Append(chr);
			}
		} else {
			var notFirst = false;
			foreach( var token in tokens ) {
				if( notFirst ) {
					_ = sb.Append(", ");
				}
				_ = sb.Append(token);
				notFirst = true;
			}
		}
		_ = sb.Append('"');
	}
}
