using System;
using System.Diagnostics.CodeAnalysis;

namespace Pidgin;

public static partial class Parser {

	/// <summary>
	/// If <paramref name="parser"/> succeeds, <c>Lookahead(parser)</c> backtracks,
	/// behaving as if <paramref name="parser"/> had not consumed any input.
	/// No backtracking is performed upon failure.
	/// </summary>
	/// <param name="parser">The parser to look ahead with</param>
	/// <returns>A parser which rewinds the input stream if <paramref name="parser"/> succeeds.</returns>
	public static Parser<TToken, T> Lookahead<TToken, T>( Parser<TToken, T> parser ) {
		ArgumentNullException.ThrowIfNull(parser);
		return new LookaheadParser<TToken, T>(parser);
	}
}

internal sealed class LookaheadParser<TToken, T> : Parser<TToken, T> {
	private readonly Parser<TToken, T> _parser;

	public LookaheadParser( Parser<TToken, T> parser ) => _parser = parser;

	public override sealed bool TryParse( ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out T result ) {
		state.PushBookmark();

		if( _parser.TryParse(ref state, ref expecteds, out result) ) {
			state.Rewind();
			return true;
		}

		state.PopBookmark();
		return false;
	}
}
