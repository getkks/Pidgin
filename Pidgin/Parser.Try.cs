using System;
using System.Diagnostics.CodeAnalysis;

namespace Pidgin;

public static partial class Parser {

	/// <summary>
	/// Creates a parser which applies <paramref name="parser"/> and backtracks upon failure
	/// </summary>
	/// <typeparam name="TToken">The type of tokens in the parser's input stream</typeparam>
	/// <typeparam name="T">The return type of the parser</typeparam>
	/// <param name="parser">The parser</param>
	/// <returns>A parser which applies <paramref name="parser"/> and backtracks upon failure</returns>
	public static Parser<TToken, T> Try<TToken, T>( Parser<TToken, T> parser ) {
		ArgumentNullException.ThrowIfNull(parser);
		return new TryParser<TToken, T>(parser);
	}
}

internal sealed class TryParser<TToken, T> : Parser<TToken, T> {
	private readonly Parser<TToken, T> _parser;

	public TryParser( Parser<TToken, T> parser ) => _parser = parser;

	public override sealed bool TryParse( ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out T result ) {
		// start buffering the input
		state.PushBookmark();
		if( !_parser.TryParse(ref state, ref expecteds, out result) ) {
			// return to the start of the buffer and discard the bookmark
			state.Rewind();
			return false;
		}

		// discard the buffer
		state.PopBookmark();
		return true;
	}
}
