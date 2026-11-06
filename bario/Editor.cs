using bario.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	public class Editor {
		public bool active = false;
		public string selectedTile = "glass";
		private readonly Level level;

		public Editor(RenderContext ctx, Level level) {
			Texture placement = Texture.LoadFromFile("res/textures/placement.png", ctx.Renderer);
			TextureRegistry.Register(placement, "placement");
			Texture frame = Texture.LoadFromFile("res/textures/selectedTileFrame.png", ctx.Renderer);
			TextureRegistry.Register(frame, "selectedTileFrame");
			this.level = level;
		}

		public void Update(Input input, RenderContext ctx) {
			if (input.GetKeyJustPressed(SDL3.SDL.Keycode.E)) {
				active = !active;
			}

			if (!active) return;

			Vec2 worldPos = (input.GetMousePosition() / 4f) + new Vec2(ctx.Camera.Left, ctx.Camera.Top);
			(int tileX, int tileY) = WorldToTile(worldPos);

			if (input.GetMouseButtonPressed(MouseButton.Left)) {
				if (TileRegistry.GetTile(selectedTile) is Tile tile) {
					level.TileMap.Foreground.SetTile(tileX, tileY, tile.RegistryID);
				}
			} else if (input.GetMouseButtonPressed(MouseButton.Right)) {
				level.TileMap.Foreground.ClearTile(tileX, tileY);
			}
		}

		public void Render(Input input, RenderContext ctx) {
			if (!active) return;

			Vec2 worldPos = (input.GetMousePosition() / 4f) + new Vec2(ctx.Camera.Left, ctx.Camera.Top);
			(int tileX, int tileY) = WorldToTile(worldPos);

			ctx.DrawTexture(TextureRegistry.GetTexture("placement"), new(tileX * 16f, tileY * 16f));

			ctx.DrawTexture(TextureRegistry.GetTexture("selectedTileFrame"), new(8f + ctx.Camera.Left, 8f + ctx.Camera.Top));
			if (TileRegistry.GetTile(selectedTile) is Tile tile) {
				ctx.DrawTexture(TextureRegistry.GetTexture(tile.texture), new(12f + ctx.Camera.Left, 12f + ctx.Camera.Top));
			}

			if (input.GetKeyPressed(SDL3.SDL.Keycode.G)) {
				DrawPickBlockScreen(input, ctx);
			}
		}

		public static (int x, int y) WorldToTile(Vec2 worldPos) {
			return (
				(int)MathF.Floor(worldPos.x / 16f),
				(int)MathF.Floor(worldPos.y / 16f)
			);
		}

		private void DrawPickBlockScreen(Input input, RenderContext ctx) {
			float size = MathF.Min(ctx.Height - 200f, ctx.Width - 200f);
		}
	}
}
