using System;
using System.Diagnostics.CodeAnalysis;

using Pidgin.Configuration;

namespace Pidgin;

internal interface IChainer<in T, out U> {

	void Apply( T value );

	U GetResult();

	void OnError();
}

public partial class Parser<TToken, T> {

	internal Parser<TToken, U> ChainAtLeastOnce<U, TChainer>( Func<IConfiguration<TToken>, TChainer> factory ) where TChainer : struct, IChainer<T, U>
		=> new ChainAtLeastOnceLParser<TToken, T, U, TChainer>(this, factory);
}

internal class ChainAtLeastOnceLParser<TToken, T, U, TChainer> : Parser<TToken, U> where TChainer : struct, IChainer<T, U> {
	private readonly Func<IConfiguration<TToken>, TChainer> _factory;
	private readonly Parser<TToken, T> _parser;

	public ChainAtLeastOnceLParser( Parser<TToken, T> parser, Func<IConfiguration<TToken>, TChainer> factory ) {
		_parser = parser;
		_factory = factory;
	}

	public override sealed bool TryParse( ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out U result ) {
		result = default;
		if( !_parser.TryParse(ref state, ref expecteds, out var result1) ) {
			// state.Error set by _parser
			return false;
		}

		var chainer = _factory(state.Configuration);
		chainer.Apply(result1);

		var lastStartLoc = state.Location;
		var childExpecteds = new PooledList<Expected<TToken>>(state.Configuration.ArrayPoolProvider.GetArrayPool<Expected<TToken>>());
		while( _parser.TryParse(ref state, ref childExpecteds, out var childResult) ) {
			var endLoc = state.Location;
			childExpecteds.Clear();
			if( endLoc <= lastStartLoc ) {
				childExpecteds.Dispose();
				chainer.OnError();
				throw new InvalidOperationException("Many() used with a parser which consumed no input");
			}
			chainer.Apply(childResult);

			lastStartLoc = endLoc;
		}
		var lastParserConsumedInput = state.Location > lastStartLoc;
		if( lastParserConsumedInput ) {
			expecteds.AddRange(childExpecteds.AsSpan());
			childExpecteds.Dispose();
			// the most recent parser failed after consuming input
			// state.Error set by _parser
			chainer.OnError();
			return false;
		}

		childExpecteds.Dispose();
		result = chainer.GetResult();
		return true;
	}
}
