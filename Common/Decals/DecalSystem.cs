using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Chunks;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Decals;

public struct DecalInfo
{
	public Texture2D Texture = TextureAssets.BlackTile.Value;
	public Rectangle DstRect;
	public Rectangle? SrcRect;
	public Color Color = Color.White;
	public float Rotation;
	public bool IfChunkExists;

	public (Vector2 point, Vector2Int halfSize) PointSize {
		set {
			var size = value.halfSize * 2;
			var offset = value.halfSize - Vector2.One;
			DstRect = new Rectangle((int)(value.point.X - offset.X), (int)(value.point.Y - offset.Y), size.X, size.Y);
		}
	}

	public DecalInfo() { }
}

[Autoload(Side = ModSide.Client)]
public sealed class DecalSystem : ModSystem
{
	public static readonly BlendState DefaultBlendState = BlendState.AlphaBlend;
	public static readonly ConfigEntry<bool> EnableDecals = new(ConfigSide.ClientOnly, true, "BloodAndGore");
	
	private static readonly List<DecalStyle> decalStyles = new();

	public static Asset<Effect>? BloodShader { get; private set; }

	public static ReadOnlySpan<DecalStyle> DecalStyles => CollectionsMarshal.AsSpan(decalStyles);

	public override void Load()
	{
		BloodShader = Mod.Assets.Request<Effect>("Assets/Shaders/Blood");

		DecalStyle.RegisterDefaultStyles();
	}

	public static void RegisterStyle(DecalStyle style)
	{
		if (style.Id != -1) {
			throw new InvalidOperationException($"Tried to register a {nameof(DecalStyle)} that was already registered!");
		}

		style.Id = decalStyles.Count;

		decalStyles.Add(style);
	}

	public static void ClearDecals(Rectangle dst)
		=> AddDecals(DecalStyle.Opaque, new DecalInfo {
			DstRect = dst,
			Color = Color.Transparent,
			IfChunkExists = true,
		});

	public static void ClearDecals(Texture2D texture, Rectangle dst, Color color)
		=> AddDecals(DecalStyle.Subtractive, new DecalInfo {
			Texture = texture,
			DstRect = dst,
			Color = color,
			IfChunkExists = true,
		});

	public static void AddDecals(DecalStyle style, in DecalInfo decal)
	{
		if (Main.dedServ || WorldGen.gen || WorldGen.IsGeneratingHardMode || !EnableDecals) { // || !ConfigSystem.local.Clientside.BloodAndGore.enableTileBlood) {
			return;
		}

		var chunkStart = new Vector2Int(
			decal.DstRect.X / TileUtils.TileSizeInPixels / Chunk.MaxChunkSize,
			decal.DstRect.Y / TileUtils.TileSizeInPixels / Chunk.MaxChunkSize
		);
		var chunkEnd = new Vector2Int(
			decal.DstRect.Right / TileUtils.TileSizeInPixels / Chunk.MaxChunkSize,
			decal.DstRect.Bottom / TileUtils.TileSizeInPixels / Chunk.MaxChunkSize
		);

		// The provided rectangle will be split between chunks, possibly into multiple draws.
		for (int chunkY = chunkStart.Y; chunkY <= chunkEnd.Y; chunkY++) {
			for (int chunkX = chunkStart.X; chunkX <= chunkEnd.X; chunkX++) {
				var chunkPoint = new Vector2Int(chunkX, chunkY);

				if (!(decal.IfChunkExists ? ChunkSystem.TryGetChunk(chunkPoint, out Chunk chunk) : ChunkSystem.TryGetOrCreateChunk(chunkPoint, out chunk!))) {
					continue;
				}

				var localDstRect = (RectFloat)decal.DstRect;

				// Clip the destination rectangle to the chunk's bounds.
				localDstRect = RectFloat.FromPoints(
					Math.Max(localDstRect.x, chunk.WorldRectangle.x),
					Math.Max(localDstRect.y, chunk.WorldRectangle.y),
					Math.Min(localDstRect.Right, chunk.WorldRectangle.Right),
					Math.Min(localDstRect.Bottom, chunk.WorldRectangle.Bottom)
				);

				// Move the destination rectangle to local space.
				localDstRect.x -= chunk.WorldRectangle.x;
				localDstRect.y -= chunk.WorldRectangle.y;
				// Divide the destination rectangle, since decal RTs have halved resolution.
				localDstRect.x /= 2;
				localDstRect.y /= 2;
				localDstRect.width /= 2;
				localDstRect.height /= 2;

				// Clip the source rectangle.
				var destinationRectInChunkSpace = RectFloat.FromPoints(((RectFloat)decal.DstRect).Points / Chunk.MaxChunkSizeInPixels);
				var clippedRectInChunkSpace = RectFloat.FromPoints(
					Math.Max(destinationRectInChunkSpace.Left, chunk.Rectangle.Left),
					Math.Max(destinationRectInChunkSpace.Top, chunk.Rectangle.Top),
					Math.Min(destinationRectInChunkSpace.Right, chunk.Rectangle.Right),
					Math.Min(destinationRectInChunkSpace.Bottom, chunk.Rectangle.Bottom)
				);

				var srcRect = decal.SrcRect ?? decal.Texture.Bounds;
				var localSrcRect = (Rectangle)new RectFloat(
					srcRect.X + (clippedRectInChunkSpace.x - destinationRectInChunkSpace.x) * (chunk.WorldRectangle.width / decal.DstRect.Width) * srcRect.Width,
					srcRect.Y + (clippedRectInChunkSpace.y - destinationRectInChunkSpace.y) * (chunk.WorldRectangle.height / decal.DstRect.Height) * srcRect.Height,
					(clippedRectInChunkSpace.width / destinationRectInChunkSpace.width) * srcRect.Width,
					(clippedRectInChunkSpace.height / destinationRectInChunkSpace.height) * srcRect.Height
				);

				// Enqueue a draw for the chunk component to do on its own.
				var chunkDecal = decal with {
					SrcRect = localSrcRect,
					DstRect = (Rectangle)localDstRect,
				};

				chunk.Components.Get<ChunkDecals>().AddDecals(style, in chunkDecal);
			}
		}
	}
}
