using System.Collections.Immutable;

namespace Pidgin;

public partial struct ParseState<TToken> {
	private bool _eof;
	private string? _message;
	private Maybe<TToken> _unexpected;

	internal int ErrorLocation {
		get; private set;
	}

	/// <summary>Sets the error. Call this when your parser fails</summary>
	public void SetError( Maybe<TToken> unexpected, bool eof, int errorLocation, string? message ) {
		_unexpected = unexpected;
		_eof = eof;
		ErrorLocation = errorLocation;
		_message = message;
	}

	internal ParseError<TToken> BuildError( ref PooledList<Expected<TToken>> expecteds ) {
		var builder = ImmutableArray.CreateBuilder<Expected<TToken>>(expecteds.Count);
		foreach( var e in expecteds ) {
			builder.Add(e);
		}
		return new ParseError<TToken>(_unexpected, _eof, builder.MoveToImmutable(), ComputeSourcePosDeltaAt(ErrorLocation), _message);
	}

	internal InternalError<TToken> GetError()
		=> new(_unexpected, _eof, ErrorLocation, _message);

	internal void SetError( InternalError<TToken> error )
				=> SetError(error.Unexpected, error.EOF, error.ErrorLocation, error.Message);
}
