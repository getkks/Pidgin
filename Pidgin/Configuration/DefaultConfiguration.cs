using System;

namespace Pidgin.Configuration;

/// <summary>
/// A default configuration for any token type
/// </summary>
/// <typeparam name="TToken">The token type</typeparam>
public class DefaultConfiguration<TToken> : IConfiguration<TToken> {

	/// <summary>
	/// The shared global instance of <see cref="DefaultConfiguration{TToken}"/>
	/// </summary>
	public static IConfiguration<TToken> Instance { get; } = new DefaultConfiguration<TToken>();

	/// <summary>
	/// Always returns <see cref="DefaultArrayPoolProvider.Instance"/>.
	/// </summary>
	public virtual IArrayPoolProvider ArrayPoolProvider { get; } = DefaultArrayPoolProvider.Instance;

	/// <summary>
	/// Always increments the column by 1.
	/// </summary>
	public virtual Func<TToken, SourcePosDelta> SourcePosCalculator {
		get;
	}
		= _ => SourcePosDelta.OneCol;
}
