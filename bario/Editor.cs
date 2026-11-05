using bario.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	public class Editor {
		public bool active = false;
		public string selectedTile = "glass";
		private readonly Level level;
		public int selectedLayer = 0;

		public Editor(RenderContext ctx, Level level) {
			Texture placement = Texture.LoadFromFile("res/textures/placement.png", ctx.Renderer);
			TextureRegistry.Register(placement, "placement");
			this.level = level;
		}

		public void Update(Input input, RenderContext ctx) {
			if (input.GetKeyJustPressed(SDL3.SDL.Keycode.E)) {
				active = !active;
			}

			if (!active) return;

			Vec2 pos = input.GetMousePosition() / 4f;
			int tileX = (int)((pos.x + ctx.Camera.Left) / 16f);
			int tileY = (int)((pos.y + ctx.Camera.Top) / 16f);

			if (tileX >= 0 && tileY >= 0 && tileX < level.TileMap.Width && tileY < level.TileMap.Height) {
				if (input.GetMouseButtonPressed(MouseButton.Left)) {
					if (TileRegistry.GetTile(selectedTile) is Tile tile) {
						level.TileMap.Layers[selectedLayer].TrySetTile(tileX, tileY, tile.RegistryID);
					}
				} else if (input.GetMouseButtonPressed(MouseButton.Right)) {
					level.TileMap.Layers[selectedLayer].TrySetTile(tileX, tileY, -1);
				}
			}
		}

		public void Render(Input input, RenderContext ctx) {
			if (!active) return;

			Vec2 pos = input.GetMousePosition() / 4f;
			int tileX = (int)((pos.x + ctx.Camera.Left) / 16f);
			int tileY = (int)((pos.y + ctx.Camera.Top) / 16f);

			//Console.WriteLine($"{tileX}, {tileY}");

			if (tileX >= 0 && tileY >= 0 && tileX < level.TileMap.Width && tileY < level.TileMap.Height) {
				ctx.DrawTexture(TextureRegistry.GetTexture("placement"), new(tileX * 16f, tileY * 16f));
			}
		}
	}
}
