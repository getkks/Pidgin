using System;

using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Pidgin.Comment;

/// <summary>
/// Contains functions to build parsers which skip over comments
/// </summary>
public static class CommentParser {

	/// <summary>
	/// Creates a parser which runs <paramref name="blockCommentStart"/>,
	/// then skips everything until <paramref name="blockCommentEnd"/>.
	/// </summary>
	/// <param name="blockCommentStart">A parser to recognise a lexeme which starts a multi-line block comment</param>
	/// <param name="blockCommentEnd">A parser to recognise a lexeme which ends a multi-line block comment</param>
	/// <returns>
	/// A parser which runs <paramref name="blockCommentStart"/>, then skips everything until <paramref name="blockCommentEnd"/>.
	/// </returns>
	public static Parser<char, Unit> SkipBlockComment<T, U>( Parser<char, T> blockCommentStart, Parser<char, U> blockCommentEnd ) {
		ArgumentNullException.ThrowIfNull(blockCommentStart);
		ArgumentNullException.ThrowIfNull(blockCommentEnd);

		return blockCommentStart
			.Then(Any.SkipUntil(blockCommentEnd))
			.Labelled("block comment");
	}

	/// <summary>
	/// Creates a parser which runs <paramref name="lineCommentStart"/>, then skips the rest of the line.
	/// </summary>
	/// <param name="lineCommentStart">A parser to recognise a lexeme which starts a line comment</param>
	/// <returns>A parser which runs <paramref name="lineCommentStart"/>, then skips the rest of the line.</returns>
	public static Parser<char, Unit> SkipLineComment<T>( Parser<char, T> lineCommentStart ) {
		ArgumentNullException.ThrowIfNull(lineCommentStart);

		var eol = Try(EndOfLine).IgnoreResult();
		return lineCommentStart
			.Then(Any.SkipUntil(End.Or(eol)))
			.Labelled("line comment");
	}

	/// <summary>
	/// Creates a parser which runs <paramref name="blockCommentStart"/>,
	/// then skips everything until <paramref name="blockCommentEnd"/>, accounting for nested comments.
	/// </summary>
	/// <param name="blockCommentStart">A parser to recognise a lexeme which starts a multi-line block comment</param>
	/// <param name="blockCommentEnd">A parser to recognise a lexeme which ends a multi-line block comment</param>
	/// <returns>
	/// A parser which runs <paramref name="blockCommentStart"/>,
	/// then skips everything until <paramref name="blockCommentEnd"/>, accounting for nested comments.
	/// </returns>
	public static Parser<char, Unit> SkipNestedBlockComment<T, U>( Parser<char, T> blockCommentStart, Parser<char, U> blockCommentEnd ) {
		ArgumentNullException.ThrowIfNull(blockCommentStart);
		ArgumentNullException.ThrowIfNull(blockCommentEnd);

		Parser<char, Unit>? parser = null;

		parser = blockCommentStart.Then(
			Rec(() => parser!).Or(Any.IgnoreResult()).SkipUntil(blockCommentEnd)
		).Labelled("block comment");

		return parser;
	}
}
