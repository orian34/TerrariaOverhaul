﻿using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Guns
{
	public class Flamethrower : Gun
	{
		private ISoundStyle fireSound;
		private SlotId soundId;

		public override bool ShouldApplyItemOverhaul(Item item) => item.useAmmo == AmmoID.Gel;

		public override void SetDefaults(Item item)
		{
			item.UseSound = null;

			fireSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Flamethrower/FlamethrowerFireLoop", 0, volume: 0.15f, pitchVariance: 0.2f);

			if (!Main.dedServ) {
				item.AddComponent<ItemUseVisualRecoil>(c => {
					c.Power = 4f;
				});

				item.AddComponent<ItemUseScreenShake>(c => {
					c.ScreenShake = new ScreenShake(3f, 0.2f);
				});
			}
		}

		public override bool? UseItem(Item item, Player player)
		{
			if (!soundId.IsValid || SoundEngine.GetActiveSound(soundId) == null) {
				soundId = SoundEngine.PlayTrackedSound(fireSound, player.Center);
			}

			return base.UseItem(item, player);
		}

		public override void HoldItem(Item item, Player player)
		{
			base.HoldItem(item, player);

			if (player.itemAnimation <= 0 && player.itemTime <= 0 && soundId.IsValid) {
				var activeSound = SoundEngine.GetActiveSound(soundId);

				activeSound?.Stop();

				soundId = SlotId.Invalid;
			} else if (soundId.IsValid) {
				var activeSound = SoundEngine.GetActiveSound(soundId);

				if (activeSound != null) {
					activeSound.Position = player.Center;
				}
			}
		}
	}
}