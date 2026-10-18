using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal class Level {
		private List<int> tiles = [];
		public int Width { get; private set; }
		public int Height { get; private set; }
		public LevelBackdrop Backdrop { get; private set; }

		public Level(int height) {
			Width = 0;
			Height = height;

			// test
			Backdrop = new LevelBackdrop.BackdropBuilder()
				.WithBackgroundFill(Common.RGB(0, 127, 127))
				.WithProp(new("tree", 48, 64))
				.WithProp(new LevelBackdrop.BackgroundProp("cloud", -32, 103).WithRandomYOffset(16))
				.Build();

			SetTile(0, 2, "smile");
			SetTile(1, 2, "smile");
			SetTile(2, 2, "smile");
		}

		private int TileIx(int x, int y) {
			return x * Height + y;
		}

		public void SetTile(int x, int y, string tile) {
			if (x >= Width) {
				for (int i = Width; i <= x; i++) {
					for (int j = 0; j < Height; j++) {
						tiles.Add(-1);
					}
				}

				Width = x + 1;
			}

			if (x < 0 || y < 0 || y >= Height) {
				throw new ArgumentOutOfRangeException($"Cannot set tile: ({x}, {y}) out of bounds {Width}x{Height}.");
			}

			int id = TileRegistry.GetID(tile);
			tiles[TileIx(x, y)] = id;
		}

		public Tile? GetTile(int x, int y) {
			if (x < 0 || y < 0 || x >= Width || y >= Height) {
				return null;
				//throw new ArgumentOutOfRangeException($"Error getting tile: ({x}, {y}) out of bounds {Width}x{Height}.");
			}

			int id = tiles[TileIx(x, y)];

			if (id >= 0) {
				return TileRegistry.GetTileByID(id);
			}

			return null;
		}

		public void Update() {

		}

		public void Render(RenderContext ctx) {
			Backdrop.Render(ctx, this);

			// float start = MathF.Floor(ctx.Camera.Left / prop.frequency) * prop.frequency;
			int minX = (int)MathF.Floor(ctx.Camera.Left / 16) * 16;
			int minY = (int)MathF.Floor(ctx.Camera.Top / 16) * 16;
			int maxX = (int)MathF.Ceiling(ctx.Camera.Right / 16) * 16;
			int maxY = (int)MathF.Ceiling(ctx.Camera.Bottom / 16) * 16;

			for (int y = minY; y <= maxY; y += 16) {
				for (int x = minX; x <= maxX; x += 16) {
					Tile? tile = GetTile(x / 16, y / 16);
					if (tile is not null) {
						Texture texture = TextureRegistry.GetTexture(tile.texture);
						ctx.DrawTexture(texture, new(x, y));
					}
				}
			}
		}

		public static void RegisterLevelTextures(RenderContext ctx) {
			Texture tree = Texture.LoadFromFile("res/textures/tree.png", ctx.Renderer);
			TextureRegistry.Register(tree, "tree");
			TextureRegistry.Register(Texture.LoadFromFile("res/textures/cloud.png", ctx.Renderer), "cloud");
			TextureRegistry.Register(Texture.LoadFromFile("res/textures/smile.png", ctx.Renderer), "smile");

			TileRegistry.Register(new Tile("smile"), "smile");
		}
	}
}
