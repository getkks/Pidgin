Pidgin
======

A lightweight, fast, and flexible parsing library for C#, developed at Stack Overflow.

Installing
----------

Pidgin is [available on Nuget](https://www.nuget.org/packages/Pidgin/). API docs are hosted [on my website](https://www.benjamin.pizza/Pidgin).

Tutorial
--------

There's a tutorial on using Pidgin to parse a subset of Prolog [on my website](https://www.benjamin.pizza/posts/2019-12-08-parsing-prolog-with-pidgin.html).

### Getting started

Pidgin is a _parser combinator library_, a lightweight, high-level, declarative tool for constructing parsers. Parsers written with parser combinators look like a high-level specification of a language's grammar, but they're expressed within a general-purpose programming language and require no special tools to produce executable code. Parser combinators are more powerful than regular expressions - they can parse a larger class of languages - but simpler and easier to use than parser generators like ANTLR.

Pidgin's core type, `Parser<TToken, T>`, represents a procedure which consumes an input stream of `TToken`s, and may either fail with a parsing error or produce a `T` as output. You can think of it as:

```csharp
delegate T? Parser<TToken, T>(IEnumerator<TToken> input);
```

In order to start building parsers we need to import two classes which contain factory methods: `Parser` and `Parser<TToken>`.

```csharp
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;  // we'll be parsing strings - sequences of characters. For other applications (eg parsing binary file formats) TToken may be some other type (eg byte).
```

### Primitive parsers

Now we can create some simple parsers. `Any` represents a parser which consumes a single character and returns that character.

```csharp
Assert.AreEqual('a', Any.ParseOrThrow("a"));
Assert.AreEqual('b', Any.ParseOrThrow("b"));
```

`Char`, an alias for `Token`, consumes a _particular_ character and returns that character. If it encounters some other character then it fails.

```csharp
Parser<char, char> parser = Char('a');
Assert.AreEqual('a', parser.ParseOrThrow("a"));
Assert.Throws<ParseException>(() => parser.ParseOrThrow("b"));
```

`Digit` parses and returns a single digit character.
```csharp
Assert.AreEqual('3', Digit.ParseOrThrow("3"));
Assert.Throws<ParseException>(() => Digit.ParseOrThrow("a"));
```

`String` parses and returns a particular string. If you give it input other than the string it was expecting it fails.

```csharp
Parser<char, string> parser = String("foo");
Assert.AreEqual("foo", parser.ParseOrThrow("foo"));
Assert.Throws<ParseException>(() => parser.ParseOrThrow("bar"));
```

`Return` (and its synonym `FromResult`) never consumes any input, and just returns the given value. Likewise, `Fail` always fails without consuming any input.

```csharp
Parser<char, int> parser = Return(3);
Assert.AreEqual(3, parser.ParseOrThrow("foo"));

Parser<char, int> parser2 = Fail<int>();
Assert.Throws<ParseException>(() => parser2.ParseOrThrow("bar"));
```

### Sequencing parsers

The power of parser combinators is that you can build big parsers out of little ones. The simplest way to do this is using `Then`, which builds a new parser representing two parsers applied sequentially (discarding the result of the first).

```csharp
Parser<char, string> parser1 = String("foo");
Parser<char, string> parser2 = String("bar");
Parser<char, string> sequencedParser = parser1.Then(parser2);
Assert.AreEqual("bar", sequencedParser.ParseOrThrow("foobar"));  // "foo" got thrown away
Assert.Throws<ParseException>(() => sequencedParser.ParseOrThrow("food"));
```

`Before` throws away the second result, not the first.

```csharp
Parser<char, string> parser1 = String("foo");
Parser<char, string> parser2 = String("bar");
Parser<char, string> sequencedParser = parser1.Before(parser2);
Assert.AreEqual("foo", sequencedParser.ParseOrThrow("foobar"));  // "bar" got thrown away
Assert.Throws<ParseException>(() => sequencedParser.ParseOrThrow("food"));
```

`Map` does a similar job, except it keeps both results and applies a transformation function to them. This is especially useful when you want your parser to return a custom data structure. (`Map` has overloads which operate on between one and eight parsers; the one-parser version also has a postfix synonym `Select`.)

```csharp
Parser<char, string> parser1 = String("foo");
Parser<char, string> parser2 = String("bar");
Parser<char, string> sequencedParser = Map((foo, bar) => bar + foo, parser1, parser2);
Assert.AreEqual("barfoo", sequencedParser.ParseOrThrow("foobar"));
Assert.Throws<ParseException>(() => sequencedParser.ParseOrThrow("food"));
```

`Bind` uses the result of a parser to choose the next parser. This enables parsing of context-sensitive languages. For example, here's a parser which parses any character repeated twice.

```csharp
/// parse any character, then parse a character matching the first character
Parser<char, char> parser = Any.Bind(c => Char(c));
Assert.AreEqual('a', parser.ParseOrThrow("aa"));
Assert.AreEqual('b', parser.ParseOrThrow("bb"));
Assert.Throws<ParseException>(() => parser.ParseOrThrow("ab"));
```

Pidgin parsers support LINQ query syntax. It may be easier to see what the above example does when it's written out using LINQ:

```csharp
Parser<char, char> parser =
    from c in Any
    from c2 in Char(c)
    select c2;
```

Parsers written like this look like a simple imperative script. "Run the `Any` parser and name its result `c`, then run `Char(c)` and name its result `c2`, then return `c2`."

### Choosing from alternatives

`Or` represents a parser which can parse one of two alternatives. It runs the left parser first, and if it fails it tries the right parser.

```csharp
Parser<char, string> parser = String("foo").Or(String("bar"));
Assert.AreEqual("foo", parser.ParseOrThrow("foo"));
Assert.AreEqual("bar", parser.ParseOrThrow("bar"));
Assert.Throws<ParseException>(() => parser.ParseOrThrow("baz"));
```

`OneOf` is equivalent to `Or`, except it takes a variable number of arguments. Here's a parser which is equivalent to the one using `Or` above:

```csharp
Parser<char, string> parser = OneOf(String("foo"), String("bar"));
```

If one of `Or` or `OneOf`'s component parsers fails _after consuming input_, the whole parser will fail.

```csharp
Parser<char, string> parser = String("food").Or(String("foul"));
Assert.Throws<ParseException>(() => parser.ParseOrThrow("foul"));  // why didn't it choose the second option?
```

What happened here? When a parser successfully parses a character from the input stream, it advances the input stream to the next character. `Or` only chooses the next alternative if the given parser fails _without consuming any input_; Pidgin does not perform any lookahead or backtracking by default. Backtracking is enabled using the `Try` function.

```csharp
// apply Try to the first option, so we can return to the beginning if it fails
Parser<char, string> parser = Try(String("food")).Or(String("foul"));
Assert.AreEqual("foul", parser.ParseOrThrow("foul"));
```

### Recursive grammars

Almost any non-trivial programming language, markup language, or data interchange language will feature some sort of recursive structure. C# doesn't support recursive values: a recursive referral to a variable currently being initialised will return `null`. So we need some sort of deferred execution of recursive parsers, which Pidgin enables using the `Rec` combinator. Here's a simple parser which parses arbitrarily nested parentheses with a single digit inside them.

```csharp
Parser<char, char> expr = null;
Parser<char, char> parenthesised = Char('(')
    .Then(Rec(() => expr))  // using a lambda to (mutually) recursively refer to expr
    .Before(Char(')'));
expr = Digit.Or(parenthesised);
Assert.AreEqual('1', expr.ParseOrThrow("1"));
Assert.AreEqual('1', expr.ParseOrThrow("(1)"));
Assert.AreEqual('1', expr.ParseOrThrow("(((1)))"));
```

However, Pidgin does not support left recursion. A parser must consume some input before making a recursive call. The following example will produce a stack overflow because a recursive call to `arithmetic` occurs before any input can be consumed by `Digit` or `Char('+')`:

```csharp
Parser<char, int> arithmetic = null;
Parser<char, int> addExpr = Map(
    (x, _, y) => x + y,
    Rec(() => arithmetic),
    Char('+'),
    Rec(() => arithmetic)
);
arithmetic = addExpr.Or(Digit.Select(d => (int)char.GetNumericValue(d)));

arithmetic.Parse("2+2");  // stack overflow!
```

### Derived combinators

Another powerful element of this programming model is that you can write your own functions to compose parsers. Pidgin contains a large number of higher-level combinators, built from the primitives outlined above. For example, `Between` runs a parser surrounded by two others, keeping only the result of the central parser.

```csharp
Parser<TToken, T> InBraces<TToken, T, U, V>(this Parser<TToken, T> parser, Parser<TToken, U> before, Parser<TToken, V> after)
    => before.Then(parser).Before(after);
```

### Parsing expressions

Pidgin features operator-precedence parsing tools, for parsing expression grammars with associative infix operators. The `ExpressionParser` class builds a parser from a parser to parse a single expression term and a table of operators with rules to combine expressions.

### More examples

Examples, such as parsing (a subset of) JSON and XML into document structures, can be found in the `Pidgin.Examples` project.

Tips
----

### A note on variance

Why doesn't this code compile?

```csharp
class Base {}
class Derived : Base {}

Parser<char, Base> p = Return(new Derived());  // Cannot implicitly convert type 'Pidgin.Parser<char, Derived>' to 'Pidgin.Parser<char, Base>'
```

This would be possible if `Parser` were defined as a _covariant_ in its second type parameter (ie `interface Parser<TToken, out T>`). For the purposes of efficiency, Pidgin parsers return a struct. Structs and classes aren't allowed to have variant type parameters (only interfaces and delegates); since a Pidgin parser's return value isn't variant, nor can the parser itself.

In my experience, this crops up most frequently when returning a node of a syntax tree from a parser using `Select`. The least verbose way of rectifying this is to explicitly set `Select`'s type parameter to the supertype:

```csharp
Parser<char, Base> p = Any.Select<Base>(() => new Derived());
```

### Speed tips

Pidgin is designed to be fast and produce a minimum of garbage. A carefully written Pidgin parser can be competitive with a hand-written recursive descent parser. If you find that parsing is a bottleneck in your code, here are some tips for minimising the runtime of your parser.

* Avoid LINQ query syntax. Query comprehensions are defined by translation into core C# using `SelectMany`, however, for long queries the translation can allocate a large number of anonymous objects. This generates a lot of garbage; while those objects often won't survive the nursery it's still preferable to avoid allocating them!
* Avoid backtracking where possible. If consuming a streaming input like a `TextReader` or an `IEnumerable`, `Try` _buffers_ its input to enable backtracking, which can be expensive.
* Use specialised parsers where possible: the provided `Skip*` parsers can be used when the result of parsing is not required. They typically run faster than their counterparts because they don't need to save the values generated.
* Build your parser statically where possible. Pidgin is designed under the assumption that parser scripts are executed more than they are written; building a parser can be an expensive operation.
* Avoid `Bind` and `SelectMany` where possible. Many practical grammars are _context-free_ and can therefore be written purely with `Map`. If you do have a context-sensitive grammar, it may make sense to parse it in a context-free fashion and then run a semantic checker over the result.

Comparison to other tools
-------------------------

### Pidgin vs Sprache

[Sprache](https://github.com/sprache/Sprache) is another parser combinator library for C# and served as one of the sources of inspiration for Pidgin. Pidgin's API is somewhat similar to that of Sprache, but Pidgin aims to improve on Sprache in a number of ways:

* Sprache's input must be a string. This makes it inappropriate for parsing binary protocols or tokenised inputs. Pidgin supports input tokens of arbitrary type.
* Sprache's input must be a string - an _in-memory_ array of characters. Pidgin supports streaming inputs.
* Sprache automatically backtracks on failure. Pidgin uses a special combinator to enable backtracking because backtracking can be a costly operation.
* Pidgin comes bundled with operator-precedence tools for parsing expression languages with associative infix operators.
* Pidgin is faster and allocates less memory than Sprache.
* Pidgin has more documentation coverage than Sprache.

### Pidgin vs FParsec

[FParsec](https://github.com/stephan-tolksdorf/fparsec) is a parser combinator library for F# based on [Parsec](https://hackage.haskell.org/package/parsec-3.1.11).

* FParsec is an F# library and consuming it from C# can be awkward. Pidgin is implemented in pure C#, and is designed for C# consumers.
* FParsec only supports character input streams.
* FParsec supports stateful parsing - it has an extra type parameter for an arbitrary user-defined state - which can make it easier to parse context-sensitive grammars.
* FParsec is faster than Pidgin (though we're catching up!)

### Benchmarks

This is how Pidgin compares to other tools in terms of performance. The benches can be found in the `Pidgin.Bench` project.

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1466 (21H1/May2021Update)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.200-preview.22055.15
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
  DefaultJob : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
```

#### `ExpressionBench`

|              Method |          Mean |        Error |       StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
|-------------------- |--------------:|-------------:|-------------:|------:|--------:|-------:|----------:|
|   LongInfixL_Pidgin | 261,991.22 ns | 2,222.554 ns | 2,078.978 ns | 10.81 |    0.08 |      - |     137 B |
|   LongInfixR_Pidgin | 279,520.94 ns | 2,166.545 ns | 1,809.163 ns | 11.53 |    0.08 |      - |     129 B |
|  LongInfixL_FParsec |  24,237.13 ns |    88.340 ns |    78.311 ns |  1.00 |    0.00 |      - |     200 B |
|  LongInfixR_FParsec |  31,949.79 ns |   135.188 ns |   126.455 ns |  1.32 |    0.01 |      - |     200 B |
|                     |               |              |              |       |         |        |           |
|  ShortInfixL_Pidgin |     863.92 ns |     6.357 ns |     5.636 ns | 13.30 |    0.15 | 0.0076 |     136 B |
|  ShortInfixR_Pidgin |     920.76 ns |     8.123 ns |     7.598 ns | 14.17 |    0.16 | 0.0076 |     128 B |
| ShortInfixL_FParsec |      64.97 ns |     0.502 ns |     0.469 ns |  1.00 |    0.00 | 0.0119 |     200 B |
| ShortInfixR_FParsec |      64.70 ns |     0.485 ns |     0.454 ns |  1.00 |    0.01 | 0.0119 |     200 B |

#### `JsonBench`

|              Method |       Mean |    Error |   StdDev | Ratio | RatioSD |    Gen 0 |   Gen 1 | Allocated |
|-------------------- |-----------:|---------:|---------:|------:|--------:|---------:|--------:|----------:|
|      BigJson_Pidgin |   235.6 μs |  2.56 μs |  2.27 μs |  1.00 |    0.00 |   6.1035 |  0.9766 |    102 KB |
|     BigJson_Sprache | 1,625.5 μs | 17.24 μs | 16.12 μs |  6.90 |    0.09 | 322.2656 | 74.2188 |  5,274 KB |
|  BigJson_Superpower | 1,063.0 μs |  7.74 μs |  7.24 μs |  4.51 |    0.05 |  54.6875 |  9.7656 |    913 KB |
|     BigJson_FParsec |   176.5 μs |  1.42 μs |  1.32 μs |  0.75 |    0.01 |  14.4043 |  1.7090 |    236 KB |
|                     |            |          |          |       |         |          |         |           |
|     LongJson_Pidgin |   196.8 μs |  1.25 μs |  1.17 μs |  1.00 |    0.00 |   6.3477 |  0.9766 |    104 KB |
|    LongJson_Sprache | 1,339.3 μs |  6.18 μs |  5.78 μs |  6.81 |    0.05 | 259.7656 | 46.8750 |  4,245 KB |
| LongJson_Superpower |   872.6 μs |  3.58 μs |  3.34 μs |  4.43 |    0.03 |  42.9688 |  6.8359 |    707 KB |
|    LongJson_FParsec |   146.2 μs |  0.73 μs |  0.65 μs |  0.74 |    0.01 |  13.9160 |  1.4648 |    230 KB |
|                     |            |          |          |       |         |          |         |           |
|     DeepJson_Pidgin |   217.9 μs |  2.41 μs |  2.25 μs |  1.00 |    0.00 |  11.9629 |  2.9297 |    197 KB |
|    DeepJson_Sprache | 1,049.7 μs |  4.94 μs |  4.62 μs |  4.82 |    0.05 | 175.7813 | 64.4531 |  2,898 KB |
|    DeepJson_FParsec |   129.1 μs |  0.69 μs |  0.65 μs |  0.59 |    0.01 |  11.4746 |  0.9766 |    190 KB |
|                     |            |          |          |       |         |          |         |           |
|     WideJson_Pidgin |   126.7 μs |  0.79 μs |  0.74 μs |  1.00 |    0.00 |   2.9297 |  0.2441 |     48 KB |
|    WideJson_Sprache |   759.5 μs |  2.65 μs |  2.47 μs |  5.99 |    0.03 | 168.9453 | 25.3906 |  2,761 KB |
| WideJson_Superpower |   543.0 μs |  2.93 μs |  2.74 μs |  4.29 |    0.02 |  27.3438 |  1.9531 |    460 KB |
|    WideJson_FParsec |   100.2 μs |  0.50 μs |  0.46 μs |  0.79 |    0.01 |   6.3477 |  0.6104 |    105 KB |
