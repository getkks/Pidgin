using System;

namespace Pidgin;

public partial class Parser<TToken, T> {

	/// <summary>
	/// For debugging use.
	///
	/// Creates a new parser which runs the current parser and prints the given message to the console.
	/// </summary>
	/// <returns>A parser which runs the current parser and prints the given message to the console.</returns>
	public Parser<TToken, T> Trace( Func<T, string> message ) {
		ArgumentNullException.ThrowIfNull(message);
		return Select(x => {
			Console.WriteLine(message(x));
			return x;
		});
	}

	/// <summary>
	/// For debugging use.
	///
	/// Creates a new parser which runs the current parser and prints the given message to the console.
	/// </summary>
	/// <returns>A parser which runs the current parser and prints the given message to the console.</returns>
	public Parser<TToken, T> Trace( string message ) {
		ArgumentNullException.ThrowIfNull(message);
		return Trace(_ => message);
	}

	/// <summary>
	/// For debugging use.
	///
	/// Creates a new parser which runs the current parser and prints the result to the console.
	/// </summary>
	/// <returns>A parser which runs the current parser and prints the result to the console.</returns>
	public Parser<TToken, T> TraceResult() => Trace(x => x!.ToString()!);
}
