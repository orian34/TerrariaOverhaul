using System;
using System.Runtime.InteropServices;

namespace TerrariaOverhaul.Utilities;

public ref struct CounterHandle
{
	// Can't use ref fields in C# 10.0
	private Span<uint> counter;

	public CounterHandle(ref uint counter)
	{
		this.counter = MemoryMarshal.CreateSpan(ref counter, 1);

		checked {
			counter++;
		}
	}

	public void Dispose()
	{
		if (counter.Length != 0) {
			checked {
				counter[0]--;
			}

			counter = default;
		}
	}
}
