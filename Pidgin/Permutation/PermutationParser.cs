using System;
using System.Collections.Immutable;
using System.Linq;

namespace Pidgin.Permutation;

/// <summary>
/// Contains tools for running sequences of parsers in an order-insensitive manner
/// </summary>
public static class PermutationParser {

	/// <summary>
	/// Creates an empty instance of <see cref="PermutationParser{TToken, T}"/>.
	/// </summary>
	/// <typeparam name="TToken">The type of tokens to be consumed by the permutation parser.</typeparam>
	/// <returns></returns>
	public static PermutationParser<TToken, Unit> Create<TToken>()
		=> new(
			() => Unit.Value,
			ImmutableList.Create<PermutationParserBranch<TToken, Unit>>()
		);
}

/// <summary>
/// A permutation parser represents a collection of parsers which can be run in an order-insensitive manner.
///
/// Declaration modifiers in C# are an example of an order-insensitive grammar.
/// Modifiers can appear in any order: <c>protected internal static readonly int x;</c>
/// means the same as <c>internal readonly protected static int x;</c>.
///
/// Usage of this class involves calling <see cref="Add{U}(Parser{TToken, U})"/>
/// or <see cref="AddOptional{U}(Parser{TToken, U}, U)"/> to add parsers to the permutation parser,
/// and then calling <see cref="Build"/> to create a parser which runs them in an order-insensitive manner
/// and returns the results in a nested tuple.
///
/// Note that the parsers that are added to the permutation parser must always consume input before succeeding.
/// If a parser succeeds on empty input the permutation parser will not work correctly.
/// If you want to run a parser optionally, use <see cref="AddOptional{U}(Parser{TToken, U}, U)"/>.
///
/// This class is immutable.
/// </summary>
public sealed class PermutationParser<TToken, T> {
	private readonly Func<T>? _exit;
	private readonly ImmutableList<PermutationParserBranch<TToken, T>> _forest;

	internal PermutationParser( Func<T>? exit, ImmutableList<PermutationParserBranch<TToken, T>> forest ) {
		_exit = exit;
		_forest = forest;
	}

	/// <summary>
	/// Adds a parser to the collection.
	/// </summary>
	/// <param name="parser">The parser to add to the collection.</param>
	/// <returns>
	/// A new permutation parser representing the current collection of parsers with <paramref name="parser"/> added.
	/// </returns>
	public PermutationParser<TToken, (T, U)> Add<U>( Parser<TToken, U> parser )
		=> Add(parser, ValueTuple.Create);

	/// <summary>
	/// Adds a parser to the collection.
	/// </summary>
	/// <param name="parser">The parser to add to the collection.</param>
	/// <param name="resultSelector">
	/// A transformation function to apply to the result of the current permutation parser and the result of <paramref name="parser"/>.
	/// </param>
	/// <returns>
	/// A new permutation parser representing the current collection of parsers with <paramref name="parser"/> added.
	/// </returns>
	public PermutationParser<TToken, R> Add<U, R>( Parser<TToken, U> parser, Func<T, U, R> resultSelector ) {
		ArgumentNullException.ThrowIfNull(parser);
		ArgumentNullException.ThrowIfNull(resultSelector);
		return new(
			null,
			ConvertForestAndAddParser(b => b.Add(parser, resultSelector), parser, resultSelector)
		);
	}

	/// <summary>
	/// Adds an optional parser to the collection.
	///
	/// The resulting permutation parser will successfully parse a phrase even if <paramref name="parser"/> never succeeds.
	/// In that case, <see cref="Maybe.Nothing{T}"/> will be returned.
	/// </summary>
	/// <param name="parser">The parser to add to the collection.</param>
	/// <returns>
	/// A new permutation parser representing the current collection of parsers with <paramref name="parser"/> added optionally.
	/// </returns>
	public PermutationParser<TToken, (T, Maybe<U>)> AddOptional<U>( Parser<TToken, U> parser )
		=> AddOptional(parser.Select(Maybe.Just), Maybe.Nothing<U>());

	/// <summary>
	/// Adds an optional parser to the collection.
	///
	/// The resulting permutation parser will successfully parse a phrase even if <paramref name="parser"/> never succeeds.
	/// In that case, <paramref name="defaultValue"/> will be returned.
	/// </summary>
	/// <param name="parser">The parser to add to the collection.</param>
	/// <param name="defaultValue">A default value to return if <paramref name="parser"/> fails.</param>
	/// <returns>
	/// A new permutation parser representing the current collection of parsers with <paramref name="parser"/> added optionally.
	/// </returns>
	public PermutationParser<TToken, (T, U)> AddOptional<U>( Parser<TToken, U> parser, U defaultValue )
		=> AddOptional(parser, () => defaultValue);

	/// <summary>
	/// Adds an optional parser to the collection.
	///
	/// The resulting permutation parser will successfully parse a phrase even if <paramref name="parser"/> never succeeds.
	/// In that case, <paramref name="defaultValueFactory"/> will be called to get a value to return.
	/// </summary>
	/// <param name="parser">The parser to add to the collection.</param>
	/// <param name="defaultValueFactory">A factory for a default value to return if <paramref name="parser"/> fails.</param>
	/// <returns>
	/// A new permutation parser representing the current collection of parsers with <paramref name="parser"/> added optionally.
	/// </returns>
	public PermutationParser<TToken, (T, U)> AddOptional<U>( Parser<TToken, U> parser, Func<U> defaultValueFactory )
		=> AddOptional(parser, defaultValueFactory, ValueTuple.Create);

	/// <summary>
	/// Adds an optional parser to the collection.
	///
	/// The resulting permutation parser will successfully parse a phrase even if <paramref name="parser"/> never succeeds.
	/// In that case, <see cref="Maybe.Nothing{T}"/> will be returned.
	/// </summary>
	/// <param name="parser">The parser to add to the collection.</param>
	/// <param name="resultSelector">
	/// A transformation function to apply to the result of the current permutation parser and the result of <paramref name="parser"/>.
	/// </param>
	/// <returns>
	/// A new permutation parser representing the current collection of parsers with <paramref name="parser"/> added optionally.
	/// </returns>
	public PermutationParser<TToken, R> AddOptional<U, R>( Parser<TToken, U> parser, Func<T, Maybe<U>, R> resultSelector )
		=> AddOptional(parser.Select(Maybe.Just), () => Maybe.Nothing<U>(), resultSelector);

	/// <summary>
	/// Adds an optional parser to the collection.
	///
	/// The resulting permutation parser will successfully parse a phrase even if <paramref name="parser"/> never succeeds.
	/// In that case, <paramref name="defaultValue"/> will be returned.
	/// </summary>
	/// <param name="parser">The parser to add to the collection.</param>
	/// <param name="defaultValue">A default value to return if <paramref name="parser"/> fails.</param>
	/// <param name="resultSelector">
	/// A transformation function to apply to the result of the current permutation parser and the result of <paramref name="parser"/>.
	/// </param>
	/// <returns>
	/// A new permutation parser representing the current collection of parsers with <paramref name="parser"/> added optionally.
	/// </returns>
	public PermutationParser<TToken, R> AddOptional<U, R>( Parser<TToken, U> parser, U defaultValue, Func<T, U, R> resultSelector )
		=> AddOptional(parser, () => defaultValue, resultSelector);

	/// <summary>
	/// Adds an optional parser to the collection.
	///
	/// The resulting permutation parser will successfully parse a phrase even if <paramref name="parser"/> never succeeds.
	/// In that case, <paramref name="defaultValueFactory"/> will be called to get a value to return.
	/// </summary>
	/// <param name="parser">The parser to add to the collection.</param>
	/// <param name="defaultValueFactory">A factory for a default value to return if <paramref name="parser"/> fails.</param>
	/// <param name="resultSelector">
	/// A transformation function to apply to the result of the current permutation parser and the result of <paramref name="parser"/>.
	/// </param>
	/// <returns>
	/// A new permutation parser representing the current collection of parsers with <paramref name="parser"/> added optionally.
	/// </returns>
	public PermutationParser<TToken, R> AddOptional<U, R>( Parser<TToken, U> parser, Func<U> defaultValueFactory, Func<T, U, R> resultSelector ) {
		ArgumentNullException.ThrowIfNull(parser);
		ArgumentNullException.ThrowIfNull(defaultValueFactory);
		ArgumentNullException.ThrowIfNull(resultSelector);

		var this_exit = _exit;
		return new PermutationParser<TToken, R>(
			this_exit == null ? null : () => resultSelector(this_exit!(), defaultValueFactory()),
			ConvertForestAndAddParser(b => b.AddOptional(parser, defaultValueFactory, resultSelector), parser, resultSelector)
		);
	}

	/// <summary>
	/// Creates a <see cref="Parser{TToken, T}"/> which runs the current
	/// collection of parsers in an order-insensitive manner.
	/// </summary>
	/// <returns>
	/// A <see cref="Parser{TToken, T}"/> which runs the current collection of parsers in an order-insensitive manner.
	/// </returns>
	public Parser<TToken, T> Build() {
		var forest = Parser.OneOf(_forest.Select(t => t.Build()));
		return _exit is null ? forest : forest.Or(Parser<TToken>.Return(_exit).Select(f => f()));
	}

	private ImmutableList<PermutationParserBranch<TToken, R>> ConvertForestAndAddParser<U, R>(
		Func<PermutationParserBranch<TToken, T>, PermutationParserBranch<TToken, R>> func,
		Parser<TToken, U> parser,
		Func<T, U, R> resultSelector
	) => _forest
		.ConvertAll(func)
		.Add(new PermutationParserBranchImpl<TToken, U, T, R>(parser, this, resultSelector));
}
