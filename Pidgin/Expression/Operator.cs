using System;
using System.Collections.Generic;
using System.Linq;

namespace Pidgin.Expression;

/// <summary>
/// Methods to create <see cref="OperatorTableRow{TToken, T}"/> values.
/// </summary>
public static class Operator {

	/// <summary>
	/// Creates a row in a table of operators which contains a single binary infix operator with the specified associativity.
	/// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
	/// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
	/// </summary>
	/// <param name="type">The associativity of the infix operator</param>
	/// <param name="opParser">A parser for an infix operator</param>
	/// <returns>A row in a table of operators which contains a single infix operator.</returns>
	public static OperatorTableRow<TToken, T> Binary<TToken, T>(
		BinaryOperatorType type,
		Parser<TToken, Func<T, T, T>> opParser
	) {
		ArgumentNullException.ThrowIfNull(opParser);
		return type switch {
			BinaryOperatorType.NonAssociative => InfixN(opParser),
			BinaryOperatorType.LeftAssociative => InfixL(opParser),
			BinaryOperatorType.RightAssociative => InfixR(opParser),
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};
	}

	/// <summary>
	/// Creates a row in a table of operators which contains a single left-associative infix operator.
	/// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
	/// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
	/// </summary>
	/// <param name="opParser">A parser for an infix operator</param>
	/// <returns>A row in a table of operators which contains a single infix operator.</returns>
	public static OperatorTableRow<TToken, T> InfixL<TToken, T>( Parser<TToken, Func<T, T, T>> opParser ) {
		ArgumentNullException.ThrowIfNull(opParser);
		return new(null, new[] { opParser }, null, null, null);
	}

	/// <summary>
	/// Creates a row in a table of operators which contains a single non-associative infix operator.
	/// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
	/// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
	/// </summary>
	/// <param name="opParser">A parser for an infix operator</param>
	/// <returns>A row in a table of operators which contains a single infix operator.</returns>
	public static OperatorTableRow<TToken, T> InfixN<TToken, T>( Parser<TToken, Func<T, T, T>> opParser ) {
		ArgumentNullException.ThrowIfNull(opParser);
		return new(new[] { opParser }, null, null, null, null);
	}

	/// <summary>
	/// Creates a row in a table of operators which contains a single right-associative infix operator.
	/// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
	/// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
	/// </summary>
	/// <param name="opParser">A parser for an infix operator</param>
	/// <returns>A row in a table of operators which contains a single infix operator.</returns>
	public static OperatorTableRow<TToken, T> InfixR<TToken, T>( Parser<TToken, Func<T, T, T>> opParser ) {
		ArgumentNullException.ThrowIfNull(opParser);
		return new(null, null, new[] { opParser }, null, null);
	}

	/// <summary>
	/// Creates a row in a table of operators which contains a single postfix operator.
	/// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
	/// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
	/// </summary>
	/// <param name="opParser">A parser for an postfix operator</param>
	/// <returns>A row in a table of operators which contains a single postfix operator.</returns>
	public static OperatorTableRow<TToken, T> Postfix<TToken, T>( Parser<TToken, Func<T, T>> opParser ) {
		ArgumentNullException.ThrowIfNull(opParser);
		return new(null, null, null, null, new[] { opParser });
	}

	/// <summary>
	/// Creates a row in a table of operators which contains a chainable collection of postfix operators.
	/// By default <see cref="Postfix"/> operators can only appear onc, so <c>foo()()</c> would not be parsed as "call(call(foo))".
	///
	/// This method is equivalent to:
	/// <code>
	/// Postfix(
	///     OneOf(opParsers)
	///         .AtLeastOnce()
	///         .Select&lt;Func&lt;T, T&gt;&gt;(fs => z => fs.Aggregate(z, (x, f) => f(x)))
	/// )
	/// </code>
	/// </summary>
	/// <param name="opParsers">A collection of parsers for individual postfix operators</param>
	/// <returns>A row in a table of operators which contains a chainable collection of postfix operators</returns>
	public static OperatorTableRow<TToken, T> PostfixChainable<TToken, T>( IEnumerable<Parser<TToken, Func<T, T>>> opParsers )
		=> Postfix(
			Parser.OneOf(opParsers)
				.AtLeastOncePooled()
				.Select<Func<T, T>>(fs =>
					z => {
						for( var i = 0 ; i < fs.Count ; i++ ) {
							z = fs[i](z);
						}
						fs.Dispose();
						return z;
					}
				)
		);

	/// <summary>
	/// Creates a row in a table of operators which contains a chainable collection of postfix operators.
	/// By default <see cref="Postfix"/> operators can only appear onc, so <c>foo()()</c> would not be parsed as "call(call(foo))".
	///
	/// This method is equivalent to:
	/// <code>
	/// Postfix(
	///     OneOf(opParsers)
	///         .AtLeastOnce()
	///         .Select&lt;Func&lt;T, T&gt;&gt;(fs => z => fs.Aggregate(z, (x, f) => f(x)))
	/// )
	/// </code>
	/// </summary>
	/// <param name="opParsers">A collection of parsers for individual postfix operators</param>
	/// <returns>A row in a table of operators which contains a chainable collection of postfix operators</returns>
	public static OperatorTableRow<TToken, T> PostfixChainable<TToken, T>( params Parser<TToken, Func<T, T>>[] opParsers )
		=> PostfixChainable(opParsers.AsEnumerable());

	/// <summary>
	/// Creates a row in a table of operators which contains a single prefix operator.
	/// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
	/// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
	/// </summary>
	/// <param name="opParser">A parser for an prefix operator</param>
	/// <returns>A row in a table of operators which contains a single prefix operator.</returns>
	public static OperatorTableRow<TToken, T> Prefix<TToken, T>( Parser<TToken, Func<T, T>> opParser ) {
		ArgumentNullException.ThrowIfNull(opParser);
		return new(null, null, null, new[] { opParser }, null);
	}

	/// <summary>
	/// Creates a row in a table of operators which contains a chainable collection of prefix operators.
	/// By default <see cref="Prefix"/> operators can only appear onc, so <c>- - 1</c> would not be parsed as "minus minus 1".
	///
	/// This method is equivalent to:
	/// <code>
	/// Prefix(
	///     OneOf(opParsers)
	///         .AtLeastOnce()
	///         .Select&lt;Func&lt;T, T&gt;&gt;(fs => z => fs.AggregateR(z, (f, x) => f(x)))
	/// )
	/// </code>
	/// </summary>
	/// <param name="opParsers">A collection of parsers for individual prefix operators</param>
	/// <returns>A row in a table of operators which contains a chainable collection of prefix operators</returns>
	public static OperatorTableRow<TToken, T> PrefixChainable<TToken, T>( IEnumerable<Parser<TToken, Func<T, T>>> opParsers )
		=> Prefix(
			Parser.OneOf(opParsers)
				.AtLeastOncePooled()
				.Select<Func<T, T>>(fs =>
					z => {
						for( var i = fs.Count - 1 ; i >= 0 ; i-- ) {
							z = fs[i](z);
						}
						fs.Dispose();
						return z;
					}
				)
		);

	/// <summary>
	/// Creates a row in a table of operators which contains a chainable collection of prefix operators.
	/// By default <see cref="Prefix"/> operators can only appear onc, so <c>- - 1</c> would not be parsed as "minus minus 1".
	///
	/// This method is equivalent to:
	/// <code>
	/// Prefix(
	///     OneOf(opParsers)
	///         .AtLeastOnce()
	///         .Select&lt;Func&lt;T, T&gt;&gt;(fs => z => fs.AggregateR(z, (f, x) => f(x)))
	/// )
	/// </code>
	/// </summary>
	/// <param name="opParsers">A collection of parsers for individual prefix operators</param>
	/// <returns>A row in a table of operators which contains a chainable collection of prefix operators</returns>
	public static OperatorTableRow<TToken, T> PrefixChainable<TToken, T>( params Parser<TToken, Func<T, T>>[] opParsers )
		=> PrefixChainable(opParsers.AsEnumerable());

	/// <summary>
	/// Creates a row in a table of operators which contains a single unary operator - either a prefix operator or a postfix operator.
	/// Can be combined with other <see cref="OperatorTableRow{TToken, T}"/>s using
	/// <see cref="OperatorTableRow{TToken, T}.And(OperatorTableRow{TToken, T})"/> to build a larger row.
	/// </summary>
	/// <param name="type">The type of the unary operator</param>
	/// <param name="opParser">A parser for a unary operator</param>
	/// <returns>A row in a table of operators which contains a single unary operator.</returns>
	public static OperatorTableRow<TToken, T> Unary<TToken, T>(
		UnaryOperatorType type,
		Parser<TToken, Func<T, T>> opParser
	) {
		ArgumentNullException.ThrowIfNull(opParser);
		return type switch {
			UnaryOperatorType.Prefix => Prefix(opParser),
			UnaryOperatorType.Postfix => Postfix(opParser),
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};
	}
}
