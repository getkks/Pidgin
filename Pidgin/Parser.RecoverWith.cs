using System;
using System.Diagnostics.CodeAnalysis;

namespace Pidgin;

public partial class Parser<TToken, T> {

	/// <summary>
	/// Creates a parser which runs the current parser, running <paramref name="errorHandler" /> on failure.
	/// </summary>
	/// <param name="errorHandler">A function which returns a parser to apply when the current parser fails.</param>
	/// <returns>A parser which runs the current parser, running <paramref name="errorHandler" /> on failure.</returns>
	public Parser<TToken, T> RecoverWith( Func<ParseError<TToken>, Parser<TToken, T>> errorHandler ) {
		ArgumentNullException.ThrowIfNull(errorHandler);
		return new RecoverWithParser<TToken, T>(this, errorHandler);
	}
}

internal sealed class RecoverWithParser<TToken, T> : Parser<TToken, T> {
	private readonly Func<ParseError<TToken>, Parser<TToken, T>> _errorHandler;
	private readonly Parser<TToken, T> _parser;

	public RecoverWithParser( Parser<TToken, T> parser, Func<ParseError<TToken>, Parser<TToken, T>> errorHandler ) {
		_parser = parser;
		_errorHandler = errorHandler;
	}

	// see comment about expecteds in ParseState.Error.cs
	public override sealed bool TryParse( ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out T result ) {
		var childExpecteds = new PooledList<Expected<TToken>>(state.Configuration.ArrayPoolProvider.GetArrayPool<Expected<TToken>>());
		if( _parser.TryParse(ref state, ref childExpecteds, out result) ) {
			childExpecteds.Dispose();
			return true;
		}

		var recoverParser = _errorHandler(state.BuildError(ref childExpecteds));

		childExpecteds.Dispose();

		return recoverParser.TryParse(ref state, ref expecteds, out result);
	}
}
