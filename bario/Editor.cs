using bario.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	public class Editor {
		public bool active = false;
		public int selectedTileId = 1;
		private readonly Level level;
		private bool showPickBlockScreen = false;

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

			if (input.GetKeyJustPressed(SDL3.SDL.Keycode.G)) {
				showPickBlockScreen = !showPickBlockScreen;
			}

			if (showPickBlockScreen) {
				UpdatePickBlockScreen(input);
			}

			if (!MouseCaptured(input)) {
				Vec2 worldPos = (input.GetMousePosition() / 4f) + new Vec2(ctx.Camera.Left, ctx.Camera.Top);
				(int tileX, int tileY) = WorldToTile(worldPos);

				if (input.GetMouseButtonPressed(MouseButton.Left)) {
					if (TileRegistry.GetTileByID(selectedTileId) is Tile tile) {
						level.TileMap.Foreground.SetTile(tileX, tileY, tile.RegistryID);
					}
				} else if (input.GetMouseButtonPressed(MouseButton.Right)) {
					level.TileMap.Foreground.ClearTile(tileX, tileY);
				}
			}
		}

		public void Render(Input input, RenderContext ctx) {
			if (!active) return;

			Vec2 worldPos = (input.GetMousePosition() / 4f) + new Vec2(ctx.Camera.Left, ctx.Camera.Top);
			(int tileX, int tileY) = WorldToTile(worldPos);

			ctx.DrawTexture(TextureRegistry.GetTexture("placement"), new(tileX * 16f, tileY * 16f));

			ctx.DrawTexture(TextureRegistry.GetTexture("selectedTileFrame"), new(8f + ctx.Camera.Left, 8f + ctx.Camera.Top));
			if (TileRegistry.GetTileByID(selectedTileId) is Tile tile) {
				ctx.DrawTexture(TextureRegistry.GetTexture(tile.texture), new(12f + ctx.Camera.Left, 12f + ctx.Camera.Top));
			}

			if (showPickBlockScreen) {
				DrawPickBlockScreen(input, ctx);
			}
		}

		public static (int x, int y) WorldToTile(Vec2 worldPos) {
			return (
				(int)MathF.Floor(worldPos.x / 16f),
				(int)MathF.Floor(worldPos.y / 16f)
			);
		}

		private float pickBlockScrollOffset = 0f;

		const float PANEL_X = 4f;
		const float PANEL_Y = 4f;
		const float PANEL_WIDTH = 64f;
		const float PANEL_HEIGHT = 160f;
		const float TILE_SIZE = 16f;
		const float TILE_PADDING = 2f;
		const float TILE_STEP = TILE_SIZE + TILE_PADDING;
		const float HOVER_SCALE = 1.25f;
		const float SCROLL_SPEED = 8f;

		private bool MouseCaptured(Input input) {
			Vec2 mouse = input.GetMousePosition() / 4f;

			if (mouse.x >= PANEL_X && mouse.y >= PANEL_Y && mouse.x <= PANEL_X + PANEL_WIDTH && mouse.y <= PANEL_Y + PANEL_HEIGHT) {
				return true;
			}

			return false;
		}

		private void DrawPickBlockScreen(Input input, RenderContext ctx) {
			List<Tile> tiles = TileRegistry.GetAllTiles().ToList();

			ctx.FillRect(PANEL_X, PANEL_Y, PANEL_WIDTH, PANEL_HEIGHT, 20, 20, 20, 220);
			ctx.StrokeRect(PANEL_X, PANEL_Y, PANEL_WIDTH, PANEL_HEIGHT, 80, 80, 80, 255);

			float contentHeight = tiles.Count * TILE_STEP + TILE_PADDING;
			float maxScroll = MathF.Max(0f, contentHeight - PANEL_HEIGHT);

			Vec2 mouse = input.GetMousePosition() / 4f;

			SDL3.SDL.SetRenderClipRect(ctx.Renderer, RenderContext.CreateRect(
				(int)PANEL_X,
				(int)PANEL_Y,
				(int)PANEL_WIDTH,
				(int)PANEL_HEIGHT
			));

			for (int i = 0; i < tiles.Count; i++) {
				Tile tile = tiles[i];

				float itemX = PANEL_X + TILE_PADDING;
				float itemY = PANEL_Y + TILE_PADDING + i * TILE_STEP - pickBlockScrollOffset;

				if (itemY + TILE_SIZE < PANEL_Y || itemY > PANEL_Y + PANEL_HEIGHT) continue;

				bool hovering = mouse.x >= itemX &&
								mouse.x <= itemX + TILE_SIZE &&
								mouse.y >= itemY &&
								mouse.y <= itemY + TILE_SIZE;

				bool isSelected = tile.RegistryID == selectedTileId;

				if (isSelected) {
					ctx.FillRect(itemX - 1, itemY - 1, TILE_SIZE + 2, TILE_SIZE + 2, 80, 140, 255, 180);
				} else {
					ctx.FillRect(itemX - 1, itemY - 1, TILE_SIZE + 2, TILE_SIZE + 2, 60, 60, 60, 180);
				}

				if (hovering || isSelected) {
					float scaled = TILE_SIZE * HOVER_SCALE;
					float nudge = (scaled - TILE_SIZE) / 2f;
					Texture texture = TextureRegistry.GetTexture(tile.texture);
					SDL3.SDL.SetRenderClipRect(ctx.Renderer, IntPtr.Zero); // briefly lift clip
					ctx.DrawTextureScreenSpace(texture, new(itemX - nudge, itemY - nudge), new(scaled, scaled));
					SDL3.SDL.SetRenderClipRect(ctx.Renderer, RenderContext.CreateRect(
						(int)PANEL_X, (int)PANEL_Y, (int)PANEL_WIDTH, (int)PANEL_HEIGHT
					));
				} else {
					Texture texture = TextureRegistry.GetTexture(tile.texture);
					ctx.DrawTextureScreenSpace(texture, new(itemX, itemY), new(TILE_SIZE, TILE_SIZE));
				}
			}

			SDL3.SDL.SetRenderClipRect(ctx.Renderer, IntPtr.Zero);

			if (maxScroll > 0f) {
				float trackX = PANEL_X + PANEL_WIDTH - 3f;
				float trackHeight = PANEL_HEIGHT;
				float thumbRatio = PANEL_HEIGHT / contentHeight;
				float thumbHeight = MathF.Max(6f, trackHeight * thumbRatio);
				float thumbY = PANEL_Y + (pickBlockScrollOffset / maxScroll) * (trackHeight - thumbHeight);

				ctx.FillRect(trackX, PANEL_Y, 2f, trackHeight, 40, 40, 40, 255);
				ctx.FillRect(trackX, thumbY, 2f, thumbHeight, 160, 160, 160, 255);
			}
		}

		private int hoveringTileId = -1;

		private void UpdatePickBlockScreen(Input input) {
			if (!showPickBlockScreen) return;

			List<Tile> tiles = TileRegistry.GetAllTiles().ToList();

			float contentHeight = tiles.Count * TILE_STEP + TILE_PADDING;
			float maxScroll = MathF.Max(0f, contentHeight - PANEL_HEIGHT);

			// Scroll
			pickBlockScrollOffset -= input.GetScroll() * SCROLL_SPEED;
			pickBlockScrollOffset = Math.Clamp(pickBlockScrollOffset, 0f, maxScroll);

			// Hit test — figure out which tile the mouse is over
			Vec2 mouse = input.GetMousePosition() / 4f;
			hoveringTileId = -1;

			for (int i = 0; i < tiles.Count; i++) {
				float itemX = PANEL_X + TILE_PADDING;
				float itemY = PANEL_Y + TILE_PADDING + i * TILE_STEP - pickBlockScrollOffset;

				bool hovering = mouse.x >= itemX
							&& mouse.x <= itemX + TILE_SIZE
							&& mouse.y >= itemY
							&& mouse.y <= itemY + TILE_SIZE;

				if (!hovering) continue;

				hoveringTileId = tiles[i].RegistryID;

				// GetMouseButtonJustPressed is valid here in Update
				if (input.GetMouseButtonJustPressed(MouseButton.Left)) {
					selectedTileId = tiles[i].RegistryID;
				}

				break;
			}
		}
	}
}
