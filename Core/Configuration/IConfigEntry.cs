using System;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Configuration;

public interface IConfigEntry
{
	Type ValueType { get; }
	bool IsHidden { get; }
	string Name { get; }
	string Category { get; }
	ReadOnlySpan<string> Categories { get; }
	object? Value { get; set; }
	object? LocalValue { get; set; }
	object? RemoteValue { get; set; }
	object DefaultValue { get; }
	ConfigSide Side { get; }
	LocalizedText? DisplayName { get; }
	LocalizedText? Description { get; }

	// Dumb.
	void Initialize(Mod mod, string? nameFallback = null);
}
