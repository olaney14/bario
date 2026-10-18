using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal static class TileRegistry {
		private static readonly List<Tile?> tiles = [];
		private static readonly Dictionary<string, int> registryIDs = [];

		static TileRegistry() {
			Tile tile = new("error");
			Register(tile, "error");
		}

		public static void Register(Tile tile, string name) {
			bool placed = false;
			
			for (int i = 0; i < tiles.Count; i++) {
				if (tiles[i] == null) {
					tiles[i] = tile;
					registryIDs[name] = i;
					placed = true;
					break;
				}
			}

			if (!placed) {
				tiles.Add(tile);
				registryIDs[name] = tiles.Count - 1;
			}

			tile.SetRegistryValues(name, registryIDs[name]);
		}

		public static Tile? GetTile(string name) { 
			if (registryIDs.TryGetValue(name, out int ix)) {
				return tiles[ix];
			}

			Log.Warning("Tile {A} not present", name);
			return null;
		}

		public static int GetID(string name) { 
			if (registryIDs.TryGetValue(name, out int ix)) {
				return ix;
			}

			Log.Warning("Tile {A} not present", name);
			return 0;
		}

		private static Tile ErrorTile() {
			if (tiles[0] is null || tiles[0].RegistryName != "error") {
				throw new InvalidOperationException("Error tile was not present");
			}

			return tiles[0];
		}

		public static Tile GetTileByID(int id) { 
			if (id < 0 || id >= tiles.Count) {
				Log.Warning("No tile found for id {A}", id);
				return ErrorTile();
			}

			Tile? tile = tiles[id];
			if (tile == null) {
				Log.Warning("No tile found for id {A}", id);
			}

			return tile;
		}
	}
}
