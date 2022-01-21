namespace Pidgin;

public static partial class Parser<TToken> {

	/// <summary>
	/// A parser which returns the current source position
	/// </summary>
	/// <returns>A parser which returns the current source position</returns>
	public static Parser<TToken, SourcePos> CurrentPos => CurrentSourcePosDelta.Select(d => new SourcePos(1, 1) + d);

	/// <summary>
	/// A parser which returns the current source position
	/// </summary>
	/// <returns>A parser which returns the current source position</returns>
	public static Parser<TToken, SourcePosDelta> CurrentSourcePosDelta => new CurrentPosParser<TToken>();
}

internal sealed class CurrentPosParser<TToken> : Parser<TToken, SourcePosDelta> {

	public override sealed bool TryParse( ref ParseState<TToken> state, ref PooledList<Expected<TToken>> expecteds, out SourcePosDelta result ) {
		result = state.ComputeSourcePosDelta();
		return true;
	}
}
