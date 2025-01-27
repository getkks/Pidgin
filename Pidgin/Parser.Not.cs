using System;
using System.Diagnostics.CodeAnalysis;

namespace Pidgin;

public static partial class Parser {

	/// <summary>
	/// Creates a parser which succeeds only if the given parser fails.
	/// The resulting parser does not perform any backtracking; it consumes the same amount of input as the supplied parser.
	/// Combine this function with <see cref="Parser.Try{TToken, T}(Parser{TToken, T})"/> if this behaviour is undesirable.
	/// </summary>
	/// <param name="parser">The parser that is expected to fail</param>
	/// <returns>A parser which succeeds only if the given parser fails.</returns>
	public static Parser<TToken, Unit> Not<TToken, T>( Parser<TToken, T> parser ) {
		ArgumentNullException.ThrowIfNull(parser);
		return new NegatedParser<TToken, T>(parser);
	}
}

internal sealed class NegatedParser<TToken, T> : Parser<TToken, Unit> {
	private readonly Parser<TToken, T> _parser;

	public NegatedParser( Parser<TToken, T> parser ) => _parser = parser;

	public override sealed bool TryParse( ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out Unit result ) {
		var startingLocation = state.Location;
		var token = state.HasCurrent ? Maybe.Just(state.Current) : Maybe.Nothing<TToken>();

		state.PushBookmark();  // make sure we don't throw out the buffer, we may need it to compute a SourcePos
		var childExpecteds = new PooledList<Expected<TToken>>(state.Configuration.ArrayPoolProvider.GetArrayPool<Expected<TToken>>());
		var success = _parser.TryParse(ref state, ref childExpecteds, out _);

		childExpecteds.Dispose();
		state.PopBookmark();

		if( !success ) {
			result = Unit.Value;
			return true;
		}
		state.SetError(token, false, startingLocation, null);
		result = default;
		return false;
	}
}
