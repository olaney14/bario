using System;
using System.Collections.Generic;
using System.Text;

namespace bario.Tiles {
	public enum TileMapLayer {
		Background,
		Foreground
	}

	public class TileMap {
		public TileLayer Background { get; private set; }
		public TileLayer Foreground { get; private set; }

		public TileMap() {
			Background = new TileLayer();
			Foreground = new TileLayer();
		}

		public TileLayer GetLayer(TileMapLayer layer) => layer switch {
			TileMapLayer.Foreground => Foreground,
			TileMapLayer.Background => Background,
			_ => throw new ArgumentOutOfRangeException(nameof(layer))
		};

		public bool TryGetTile(int x, int y, out Tile? tile) {
			return Foreground.TryGetTile(x, y, out tile);
		}

		public bool TryGetBackgroundTile(int x, int y, out Tile? tile) {
			return Background.TryGetTile(x, y, out tile);
		}

		public bool TryGetInstanceData(int x, int y, out TileInstanceData? data) {
			return Foreground.TryGetInstanceData(x, y, out data);
		}

		public bool GetTileSolid(int x, int y) {
			return Foreground.TryGetTile(x, y, out Tile? tile)
				&& tile != null && tile.solid;
		}

		public Collision.CollisionResult? Collide(Collision.AABB collider) {
			int tileMinX = (int)MathF.Floor(collider.min.x / 16);
			int tileMinY = (int)MathF.Floor(collider.min.y / 16);
			int tileMaxX = (int)MathF.Ceiling(collider.max.x / 16);
			int tileMaxY = (int)MathF.Ceiling(collider.max.y / 16);

			Collision.CollisionResult? best = null;

			for (int y = tileMinY; y <= tileMaxY; y++) {
				for (int x = tileMinX; x <= tileMaxX; x++) {
					if (!GetTileSolid(x, y)) continue;

					Collision.AABB tile = new() {
						min = new(x * 16f, y * 16f),
						max = new((x + 1) * 16f, (y + 1) * 16f)
					};

					if (collider.Collide(tile) is Collision.CollisionResult hit) {
						if (best == null || hit.resolution.Magnitude() < best.Value.resolution.Magnitude()) {
							best = hit;
						}
					}
				}
			}

			return best;
		}

		public Collision.SweepResult? Sweep(Collision.AABB collider, Vec2 velocity) {
			Vec2 endMin = collider.min + velocity;
			Vec2 endMax = collider.max + velocity;

			int tileMinX = (int)MathF.Floor(MathF.Min(collider.min.x, endMin.x) / 16f);
			int tileMinY = (int)MathF.Floor(MathF.Min(collider.min.y, endMin.y) / 16f);
			int tileMaxX = (int)MathF.Ceiling(MathF.Max(collider.max.x, endMax.x) / 16f);
			int tileMaxY = (int)MathF.Ceiling(MathF.Max(collider.max.y, endMax.y) / 16f);

			Collision.SweepResult? best = null;

			for (int y = tileMinY; y <= tileMaxY; y++) {
				for (int x = tileMinX; x <= tileMaxX; x++) {
					if (!GetTileSolid(x, y)) continue;

					Collision.AABB tile = new() {
						min = new(x * 16f, y * 16f),
						max = new((x + 1) * 16f, (y + 1) * 16f)
					};

					Collision.SweepResult? result = collider.Sweep(tile, velocity);

					if (result is not Collision.SweepResult hit) continue;

					int nx = (int)hit.normal.x;
					int ny = (int)hit.normal.y;

					if (GetTileSolid(x + nx, y + ny)) continue;

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
			out List<Collision.SweepResult> collisions
		) {
			Vec2 offset = new(0f, 0f);
			Vec2 remaining = velocity;
			Vec2 finalVelocity = velocity;

			collisions = [];

			for (int i = 0; i < MAX_SLIDE_COLLISIONS; i++) {
				if (remaining.MagnitudeSquared() < EPSILON * EPSILON) break;

				Collision.AABB current = collider.Offset(offset).Expand(-Collision.SKIN_WIDTH);
				Collision.SweepResult? result = Sweep(current, remaining);

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
