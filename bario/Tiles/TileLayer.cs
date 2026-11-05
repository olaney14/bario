using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace bario.Tiles {
	public class TileLayer {
		public const int EMPTY_TILE_ID = -1;
		public const int CHUNK_SIZE = 16;

		private readonly Dictionary<(int cx, int cy), Chunk> chunks = [];
		private readonly Dictionary<(int x, int y), TileInstanceData> instanceData = [];

		private class Chunk {
			public readonly int[] tiles = new int[CHUNK_SIZE * CHUNK_SIZE];

			public Chunk() {
				Array.Fill(tiles, EMPTY_TILE_ID);
			}

			public int Get(int lx, int ly) => tiles[ly * CHUNK_SIZE + lx];
			public int Set(int lx, int ly, int id) => tiles[ly * CHUNK_SIZE + lx] = id;
		}

		// Global coordinates -> Chunk pos and chunk coords
		private static (int cx, int cy, int lx, int ly) Decompose(int x, int y) {
			int cx = (int)MathF.Floor((float)x / CHUNK_SIZE);
			int cy = (int)MathF.Floor((float)y / CHUNK_SIZE);
			int lx = x - cx * CHUNK_SIZE;
			int ly = y - cy * CHUNK_SIZE;
			return (cx, cy, lx, ly);
		}

		private Chunk? GetChunk(int cx, int cy) {
			chunks.TryGetValue((cx, cy), out Chunk? chunk);
			return chunk;
		}

		private Chunk GetOrCreateChunk(int cx, int cy) {
			if (!chunks.TryGetValue((cx, cy), out Chunk? chunk)) {
				chunk = new Chunk();
				chunks[(cx, cy)] = chunk;
			}

			return chunk;
		}

		public (int minX, int minY, int maxX, int maxY) GetTileBounds() {
			if (chunks.Count == 0) return (0, 0, 0, 0);

			int minCX = int.MaxValue, minCY = int.MaxValue;
			int maxCX = int.MinValue, maxCY = int.MinValue;

			foreach (var (cx, cy) in chunks.Keys) {
				minCX = Math.Min(minCX, cx);
				minCY = Math.Min(minCY, cy);
				maxCX = Math.Max(maxCX, cx);
				maxCY = Math.Max(maxCY, cy);
			}

			return (
				minCX * CHUNK_SIZE,
				minCY * CHUNK_SIZE,
				(maxCX + 1) * CHUNK_SIZE,
				(maxCY + 1) * CHUNK_SIZE
			);
		}

		public bool TryGetTile(int x, int y, out Tile? tile) {
			var (cx, cy, lx, ly) = Decompose(x, y);
			Chunk? chunk = GetChunk(cx, cy);

			if (chunk == null) {
				tile = null;
				return false;
			}

			int id = chunk.Get(lx, ly);

			if (id == EMPTY_TILE_ID) {
				tile = null;
				return false;
			}

			tile = TileRegistry.GetTileByID(id);
			return tile != null;
		}

		public bool TryGetInstanceData(int x, int y, out TileInstanceData? outInstanceData) {
			return instanceData.TryGetValue((x, y), out outInstanceData);
		}

		// Accepts any coordinate — creates chunk on demand
		public void SetTile(int x, int y, int registryId) {
			var (cx, cy, lx, ly) = Decompose(x, y);
			GetOrCreateChunk(cx, cy).Set(lx, ly, registryId);
		}

		public bool SetTile(int x, int y, Tile tile) {
			SetTile(x, y, tile.RegistryID);
			return true;
		}

		public bool SetTile(int x, int y, string registryName) {
			Tile? tile = TileRegistry.GetTile(registryName);
			if (tile == null) return false;
			SetTile(x, y, tile.RegistryID);
			return true;
		}

		public void ClearTile(int x, int y) {
			var (cx, cy, lx, ly) = Decompose(x, y);

			// Don't create a chunk just to write empty into it
			Chunk? chunk = GetChunk(cx, cy);
			if (chunk == null) return;

			chunk.Set(lx, ly, EMPTY_TILE_ID);

			// Optionally remove the chunk if it's now entirely empty
			if (IsChunkEmpty(chunk)) chunks.Remove((cx, cy));
		}

		public IEnumerable<(int x, int y, Tile tile)> GetTilesInRange(
			int minX, int minY, int maxX, int maxY
		) {
			// Convert tile range to chunk range
			// Floor on min, ceiling on max
			// same logic as the broadphase in TileMap.Sweep
			int chunkMinX = (int)MathF.Floor((float)minX / CHUNK_SIZE);
			int chunkMinY = (int)MathF.Floor((float)minY / CHUNK_SIZE);
			int chunkMaxX = (int)MathF.Ceiling((float)maxX / CHUNK_SIZE);
			int chunkMaxY = (int)MathF.Ceiling((float)maxY / CHUNK_SIZE);

			for (int cy = chunkMinY; cy <= chunkMaxY; cy++) {
				for (int cx = chunkMinX; cx <= chunkMaxX; cx++) {
					Chunk? chunk = GetChunk(cx, cy);
					if (chunk == null) continue;

					// Clamp local iteration to only the tiles within the query range
					// so edge chunks don't yield tiles outside the camera bounds
					int worldMinX = cx * CHUNK_SIZE;
					int worldMinY = cy * CHUNK_SIZE;

					int localMinX = Math.Max(minX - worldMinX, 0);
					int localMinY = Math.Max(minY - worldMinY, 0);
					int localMaxX = Math.Min(maxX - worldMinX, CHUNK_SIZE - 1);
					int localMaxY = Math.Min(maxY - worldMinY, CHUNK_SIZE - 1);

					for (int ly = localMinY; ly <= localMaxY; ly++) {
						for (int lx = localMinX; lx <= localMaxX; lx++) {
							int id = chunk.Get(lx, ly);
							if (id == EMPTY_TILE_ID) continue;

							Tile? tile = TileRegistry.GetTileByID(id);
							if (tile == null) continue;

							yield return (worldMinX + lx, worldMinY + ly, tile);
						}
					}
				}
			}
		}

		private static bool IsChunkEmpty(Chunk chunk) {
			foreach (int id in chunk.tiles) {
				if (id != EMPTY_TILE_ID) return false;
			}

			return true;
		}
	}
}
