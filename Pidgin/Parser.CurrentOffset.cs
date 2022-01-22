namespace Pidgin;

public static partial class Parser<TToken> {

	/// <summary>
	/// A parser which returns the number of input tokens which have been consumed.
	/// </summary>
	/// <returns>A parser which returns the number of input tokens which have been consumed</returns>
<<<<<<< HEAD
	public static Parser<TToken, int> CurrentOffset => new CurrentOffsetParser<TToken>();
=======
	public static Parser<TToken, int> CurrentOffset {
		get;
	} = new CurrentOffsetParser<TToken>();
>>>>>>> 746aa4862e8ab2199671a0bd730714fe9bd680d2
}

internal sealed class CurrentOffsetParser<TToken> : Parser<TToken, int> {

	public override sealed bool TryParse( ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, out int result ) {
		result = state.Location;
		return true;
	}
}
