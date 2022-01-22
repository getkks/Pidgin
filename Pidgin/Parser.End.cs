using System.Diagnostics.CodeAnalysis;

namespace Pidgin;

public static partial class Parser<TToken> {

	/// <summary>
	/// Creates a parser which parses the end of the input stream
	/// </summary>
	/// <returns>A parser which parses the end of the input stream and returns <see cref="Unit.Value"/></returns>
<<<<<<< HEAD
	public static Parser<TToken, Unit> End  => new EndParser<TToken>();
=======
	public static Parser<TToken, Unit> End { get; } = new EndParser<TToken>();
>>>>>>> 746aa4862e8ab2199671a0bd730714fe9bd680d2
}

internal sealed class EndParser<TToken> : Parser<TToken, Unit> {

	public override sealed bool TryParse( ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, [MaybeNullWhen(false)] out Unit result ) {
		if( state.HasCurrent ) {
			state.SetError(Maybe.Just(state.Current), false, state.Location, null);
			expecteds.Add(new Expected<TToken>());
			result = default;
			return false;
		}
		result = Unit.Value;
		return true;
	}
}
