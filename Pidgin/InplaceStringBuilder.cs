using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Pidgin;

/// <summary>
/// A mutable struct! Careful!
/// </summary>
internal struct InplaceStringBuilder {
	private readonly int _capacity;
	private readonly string _value;
	private int _offset;

	public InplaceStringBuilder( int capacity ) {
		_offset = 0;
		_capacity = capacity;
		_value = new string('\0', capacity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Append( char c ) {
		if( _offset >= _capacity ) {
			throw new InvalidOperationException();
		}

		fixed( char* destination = _value ) {
			destination[_offset] = c;
			_offset++;
		}
	}

	public override string ToString() => _capacity == _offset ? _value : throw new InvalidOperationException();
}
