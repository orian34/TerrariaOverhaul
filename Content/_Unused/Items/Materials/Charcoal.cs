// Unused content, may be reintroduced in the future.
#if false
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Items.Materials;

public sealed class Charcoal : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Gel);

		Item.color = Color.White;
		Item.maxStack = 999;
		Item.ammo = 0;
	}

	/*
	protected void OverhaulInit()
	{
		item.SetTag(ItemTags.Flammable);
	}
	*/
}
#endif
