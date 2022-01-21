using System;

namespace Pidgin;

public abstract partial class Parser<TToken, T> {

	/// <summary>
	/// Creates a parser that fails if the value returned by the current parser fails to satisfy a predicate.
	/// </summary>
	/// <remarks>This function is a synonym of <see cref="Assert(Func{T, bool})"/></remarks>
	/// <param name="predicate">The predicate to apply to the value returned by the current parser</param>
	/// <returns>A parser that fails if the value returned by the current parser fails to satisfy <paramref name="predicate"/></returns>
	public Parser<TToken, T> Where( Func<T, bool> predicate ) {
		ArgumentNullException.ThrowIfNull(predicate);
		return Assert(predicate);
	}
}
