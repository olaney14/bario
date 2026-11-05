using bario.Tiles;
using SDL3;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal class Collision {
		public const float SKIN_WIDTH = 0f;

		public struct AABB {
			public Vec2 min;
			public Vec2 max;

			public Vec2 Center {
				get {
					return (min + max) / 2.0f;
				}
				set {
					Vec2 offset = value - Center;
					min += offset;
					max += offset;
				}
			}

			public Vec2 HalfExtents {
				get {
					return (max - min) / 2.0f;
				}
			}

			public readonly SDL.FRect FRect {
				get {
					return RenderContext.CreateFRect(min.x, min.y, max.x - min.x, max.y - min.y);
				}
			}

			public bool Intersects(AABB other) {
				return 
					(min.x <= other.max.x && max.x >= other.min.x) && 
					(min.y <= other.max.y && max.y >= other.min.y);
			}

			public bool Overlaps(AABB other) {
				return
					(min.x < other.max.x && max.x > other.min.x) &&
					(min.y < other.max.y && max.y > other.min.y);
			}

			public CollisionResult? Collide(AABB other) {
				if (Intersects(other)) {
					CollisionResult result = new();

					Vec2 thisCenter = Center;
					Vec2 otherCenter = other.Center;

					Vec2 thisHalf = HalfExtents;
					Vec2 otherHalf = other.HalfExtents;

					Vec2 distances = (thisCenter - otherCenter).Abs();

					Vec2 depths = (thisHalf + otherHalf) - distances;

					result.intersection = depths;

					if (depths.x < depths.y) {
						result.resolution = new Vec2(depths.x * ((thisCenter.x < otherCenter.x) ? -1f : 1f), 0f);
					} else {
						result.resolution = new Vec2(0f, depths.y * ((thisCenter.y < otherCenter.y) ? -1F : 1F));
					}

					return result;
				}

				return null;
			}

			const float EPSILON = 0.001f;

			public SweepResult? Sweep(AABB other, Vec2 velocity) {
				if (Overlaps(other)) {
					return null;
				}

				Vec2 extents = HalfExtents;

				// Essentially a ray-AABB check with the static collider expanded
				// large enough as if the moving collider was a single point
				Vec2 expandedMin = other.min - extents;
				Vec2 expandedMax = other.max + extents;

				Vec2 position = Center;

				float xEntry, xExit;
				float yEntry, yExit;

				// Sweep x
				if (velocity.x == 0f) {
					if (position.x < expandedMin.x || position.x > expandedMax.x) return null;

					xEntry = float.NegativeInfinity;
					xExit = float.PositiveInfinity;
				} else {
					float t1 = (expandedMin.x - position.x) / velocity.x;
					float t2 = (expandedMax.x - position.x) / velocity.x;

					xEntry = MathF.Min(t1, t2);
					xExit = MathF.Max(t1, t2);
				}

				// Sweep y
				if (velocity.y == 0f) {
					if (position.y < expandedMin.y || position.y > expandedMax.y) return null;

					yEntry = float.NegativeInfinity;
					yExit = float.PositiveInfinity;
				} else {
					float t1 = (expandedMin.y - position.y) / velocity.y;
					float t2 = (expandedMax.y - position.y) / velocity.y;

					yEntry = MathF.Min(t1, t2);
					yExit = MathF.Max(t1, t2);
				}

				float entryTime = MathF.Max(xEntry, yEntry);
				float exitTime = MathF.Min(xExit, yExit);

				float speed = velocity.Magnitude();
				float skinTolerance = (speed > 0f) ? SKIN_WIDTH / speed : SKIN_WIDTH;

				// If entry is after exit, beyond this frame, or exit is in the past
				if (entryTime > exitTime || exitTime < 0f || entryTime < -skinTolerance || entryTime > 1f) return null;

				// Calculate normal
				Vec2 normal;
				if (xEntry > yEntry) {
					normal = new(velocity.x > 0f ? -1f : 1f, 0f);
				} else {
					normal = new(0f, velocity.y > 0f ? -1f : 1f);
				}

				if (velocity.Dot(normal) >= 0f) return null;

				return new SweepResult {
					timeOfImpact = Math.Clamp(entryTime, 0f, 1f),
					normal = normal
				};
			}

			public AABB Offset(Vec2 offset) {
				return new() {
					min = min + offset,
					max = max + offset
				};
			}

			public AABB Expand(float by) {
				return new() {
					min = min - new Vec2(by),
					max = max + new Vec2(by)
				};
			}

			public override string ToString() {
				return $"{{ min: ({min.x}, {min.y}), max: ({max.x}, {max.y}) }}";
			}
		}

		public struct CollisionResult {
			public Vec2 intersection;
			public Vec2 resolution;
			public Vec2 normal;
		}

		public struct SweepResult {
			public float timeOfImpact;
			public Vec2 normal;
		}

		//public bool AABBIntersectingLevelTile(AABB aabb, int x, int y) {

		//}
	}
}
