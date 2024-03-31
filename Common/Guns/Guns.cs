using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Guns;

internal static class Guns
{
	public static readonly ConfigEntry<bool> EnableGunSoundReplacements = new(ConfigSide.ClientOnly, true, "Guns");
	public static readonly ConfigEntry<bool> EnableAlternateGunFiringModes = new(ConfigSide.Both, true, "Guns");
}
