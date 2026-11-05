using System;
using System.Collections.Generic;
using System.Text;

namespace bario.Tiles {
	public class Tiles {
		private static readonly List<string> registerTileTextures = [];

		public static readonly Tile SMILE = CreateAndRegisterTexture("smile");
		public static readonly Tile BLUE_BRICKS = CreateAndRegisterTexture("blue_bricks");
		public static readonly Tile BOX = CreateAndRegisterTexture("box");
		public static readonly Tile CHEMAC = CreateAndRegisterTexture("chemac");
		public static readonly Tile GLASS = CreateAndRegisterTexture("glass");

		private static Tile CreateAndRegisterTexture(string tileImage) {
			registerTileTextures.Add(tileImage);
			return TileRegistry.Register(new(tileImage), tileImage);
		}

		public static void RegisterTileTextures(IntPtr renderer) {
			foreach (string localPath in registerTileTextures) {
				string path = "res/textures/tiles/" + localPath + ".png";
				Texture texture = Texture.LoadFromFile(path, renderer);
				TextureRegistry.Register(texture, localPath);
			}
		}
	}
}
