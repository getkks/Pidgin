namespace Pidgin;

public static partial class Parser<TToken> {

	/// <summary>
	/// A parser which returns the number of input tokens which have been consumed.
	/// </summary>
	/// <returns>A parser which returns the number of input tokens which have been consumed</returns>
	public static Parser<TToken, int> CurrentOffset => new CurrentOffsetParser<TToken>();
}

internal sealed class CurrentOffsetParser<TToken> : Parser<TToken, int> {

	public override sealed bool TryParse( ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, out int result ) {
		result = state.Location;
		return true;
	}
}
