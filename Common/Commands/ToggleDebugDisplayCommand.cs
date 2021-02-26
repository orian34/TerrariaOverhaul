﻿using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Debugging;

namespace TerrariaOverhaul.Common.Commands
{
	public class ToggleDebugDisplayCommand : ModCommand
	{
		public override string Command => "oToggleDebugDisplay";
		public override CommandType Type => CommandType.Chat;

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			DebugSystem.EnableDebugRendering = !DebugSystem.EnableDebugRendering;

			Main.NewText($"Debug Display is now {(DebugSystem.EnableDebugRendering ? "On" : "Off")}");
		}
	}
}
