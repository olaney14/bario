using System;
using System.Collections.Generic;
using System.Text;

namespace bario.Tiles {
	public class TileMap {
		public List<TileLayer> Layers { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public int LayerMin { get; private set; } = 0;
		public int LayerMax { get; private set; } = 0;
		private Dictionary<int, int> layerBeginIndices = [];

		public TileMap() {
			Layers = [];
		}

		// Insert a layer in sorted Z order
		public void AddLayer(TileLayer layer) {
			Width = Math.Max(Width, layer.Width);
			Height = Math.Max(Height, layer.Height);
			LayerMax = Math.Max(LayerMax, layer.Z);
			LayerMin = Math.Min(LayerMin, layer.Z);

			int insertAt = Layers.Count;

			for (int i = 0; i < Layers.Count; i++) {
				if (layer.Z < Layers[i].Z) {
					insertAt = i; 
					break;
				}
			}

			Layers.Insert(insertAt, layer);

			for (int i = 0; i < Layers.Count; i++) {
				if (!layerBeginIndices.ContainsKey(Layers[i].Z)) {
					layerBeginIndices[Layers[i].Z] = i;
				}
			}
		}

		public IEnumerable<Tile> GetTiles(int x, int y) {
			foreach (TileLayer layer in Layers) {
				if (layer.TryGetTile(x, y, out Tile? tile) && tile != null) {
					yield return tile;
				}
			}
		}

		public IEnumerable<Tile> GetTilesByLayer(int x, int y, int z) {
			if (!layerBeginIndices.ContainsKey(z)) yield break;
			
			for (int i = layerBeginIndices[z]; i < Layers.Count; i++) {
				if (Layers[i].Z != z) yield break;

				if (Layers[i].TryGetTile(x, y, out Tile? tile) && tile != null) {
					yield return tile;
				}
			}
		}

		public IEnumerable<TileInstanceData> GetTileInstanceData(int x, int y) {
			foreach (TileLayer layer in Layers) {
				if (layer.TryGetInstanceData(x, y, out TileInstanceData? tile) && tile != null) {
					yield return tile;
				}
			}
		}

		public IEnumerable<TileInstanceData> GetTileInstanceDataByLayer(int x, int y, int z) {
			if (!layerBeginIndices.ContainsKey(z)) yield break;

			for (int i = layerBeginIndices[z]; i < Layers.Count; i++) {
				if (Layers[i].Z != z) yield break;

				if (Layers[i].TryGetInstanceData(x, y, out TileInstanceData? tile) && tile != null) {
					yield return tile;
				}
			}
		}

		public Collision.CollisionResult? Collide(Collision.AABB collider, int z) {
			int tileMinX = (int)MathF.Floor(collider.min.x / 16);
			int tileMinY = (int)MathF.Floor(collider.min.y / 16);
			int tileMaxX = (int)MathF.Ceiling(collider.max.x / 16);
			int tileMaxY = (int)MathF.Ceiling(collider.max.y / 16);

			Collision.CollisionResult? best = null;

			for (int y = tileMinY; y <= tileMaxY; y++) {
				for (int x = tileMinX; x <= tileMaxX; x++) {
					if (!GetTileSolid(x, y, z)) continue;

					Collision.AABB tile = new() {
						min = new(x * 16f, y * 16f),
						max = new((x + 1) * 16f, (y + 1) * 16f)
					};

					// Return minimum collision
					if (collider.Collide(tile) is Collision.CollisionResult hit) {
						if (best == null || hit.resolution.Magnitude() < best.Value.resolution.Magnitude()) {
							best = hit;
						}
					}
				}
			}

			return best;
		}

		// TODO: Cache this
		public bool GetTileSolid(int x, int y, int z) {
			return GetTilesByLayer(x, y, z).Any(t => {
				return t.solid;
			});
		}

		public Collision.SweepResult? Sweep(Collision.AABB collider, Vec2 velocity, int z) {
			Vec2 endMin = collider.min + velocity;
			Vec2 endMax = collider.max + velocity;

			int tileMinX = (int)MathF.Floor(MathF.Min(collider.min.x, endMin.x) / 16f);
			int tileMinY = (int)MathF.Floor(MathF.Min(collider.min.y, endMin.y) / 16f);
			int tileMaxX = (int)MathF.Ceiling(MathF.Max(collider.max.x, endMax.x) / 16f);
			int tileMaxY = (int)MathF.Ceiling(MathF.Max(collider.max.y, endMax.y) / 16f);

			Collision.SweepResult? best = null;

			for (int y = tileMinY; y <= tileMaxY; y++) {
				for (int x = tileMinX; x <= tileMaxX; x++) {
					if (!GetTileSolid(x, y, z)) continue;

					Collision.AABB tile = new() {
						min = new(x * 16f, y * 16f),
						max = new((x + 1) * 16f, (y + 1) * 16f)
					};

					Collision.SweepResult? result = collider.Sweep(tile, velocity);

					if (result is not Collision.SweepResult hit) continue;

					int nx = (int)hit.normal.x;
					int ny = (int)hit.normal.y;

					// Check for internal edges
					if (GetTileSolid(x + nx, y + ny, z)) continue;

					if (best == null || hit.timeOfImpact < best.Value.timeOfImpact) {
						best = hit;
					}
				}
			}

			return best;
		}

		const int MAX_SLIDE_COLLISIONS = 3;
		const float EPSILON = 0.0001f;

		public (Vec2 offset, Vec2 velocity) MoveAndSlide(
			Collision.AABB collider,
			Vec2 velocity,
			int z,
			out List<Collision.SweepResult> collisions
		) {
			Vec2 offset = new(0f, 0f);
			Vec2 remaining = velocity;

			Vec2 finalVelocity = velocity;

			collisions = [];

			for (int i = 0; i < MAX_SLIDE_COLLISIONS; i++) {
				if (remaining.MagnitudeSquared() < EPSILON * EPSILON) break;

				Collision.AABB current = collider.Offset(offset).Expand(-Collision.SKIN_WIDTH);

				Collision.SweepResult? result = Sweep(current, remaining, z);

				if (result is not Collision.SweepResult hit) {
					offset += remaining;
					break;
				}

				offset += remaining * hit.timeOfImpact;
				offset += hit.normal * Collision.SKIN_WIDTH;
				remaining *= 1f - hit.timeOfImpact;

				float normalVelocity = finalVelocity.Dot(hit.normal);
				if (normalVelocity < 0f) finalVelocity -= hit.normal * normalVelocity;

				float normalDisplacement = remaining.Dot(hit.normal);
				if (normalDisplacement < 0f) remaining -= hit.normal * normalDisplacement;

				collisions.Add(hit);
			}

			return (offset, finalVelocity);
		}
	}
}
