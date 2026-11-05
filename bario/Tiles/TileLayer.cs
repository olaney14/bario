using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace bario.Tiles {
	internal class TileLayer {
		public string Name { get; set; }
		// public int[,] Tiles { get; private set; }
		private int[,] tiles;
		public int Width => tiles.GetLength(1);
		public int Height => tiles.GetLength(0);
		public int Z { get; private set; }

		public const int EMPTY_TILE_ID = -1;

		private readonly Dictionary<(int x, int y), TileInstanceData> instanceData = [];

		public TileLayer(string name, int width, int height, int z) {
			Name = name;
			tiles = new int[height, width];
			Common.FillMatrix(ref tiles, -1);
			Z = z;
		}

		public void Resize(int width, int height) {
			int[,] newTiles = new int[height, width];
			int minWidth = Math.Min(this.Width, width);
			int minHeight = Math.Min(this.Height, height);

			for (int y = 0; y < minHeight; y++) {
				for (int x = 0; x < minWidth; x++) {
					newTiles[y, x] = tiles[y, x];
				}
			}

			tiles = newTiles;
		}

		public bool TryGetTile(int x, int y, out Tile? tile) {
			if (x < 0 || y < 0 || x >= Width || y >= Height) {
				tile = null;
				return false;
			}

			int id = tiles[y, x];
			if (id == EMPTY_TILE_ID) {
				tile = null;
				return false;
			}
			tile = TileRegistry.GetTileByID(id);
			return true;
		}

		public bool TryGetInstanceData(int x, int y, out TileInstanceData? outInstanceData) {
			return instanceData.TryGetValue((x, y), out outInstanceData);
		}

		public bool TrySetTile(int x, int y, Tile tile) {
			return TrySetTile(x, y, tile.RegistryID);
		}

		public bool TrySetTile(int x, int y, int registryId) {
			if (x < 0 || y < 0 || x >= Width || y >= Height) return false;

			tiles[y, x] = registryId;
			return true;
		}

		public bool TrySetTile(int x, int y, string registryName) {
			Tile? tile = TileRegistry.GetTile(registryName);
			
			if (tile == null) return false;

			return TrySetTile(x, y, tile!);
		}
	}
}
