using bario.Tiles;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static System.Net.WebRequestMethods;

namespace bario {
	public class Level {
		public float gravity = 1f;

		//private List<int> tiles = [];
		public TileMap TileMap { get; private set; }
		public LevelBackdrop Backdrop { get; private set; }

		public Level() {
			// test
			Backdrop = new LevelBackdrop.BackdropBuilder()
				.WithBackgroundFill(Common.RGB(0, 127, 127))
				.WithProp(new("tree", 48, 64))
				.WithProp(new LevelBackdrop.BackgroundProp("cloud", -32, 103).WithRandomYOffset(16))
				.Build();

			TileMap = new();

			TileLayer layer = new("Level", 16, 16, 0);
			for (int x = 0; x < 16; x++) {
				layer.TrySetTile(x, 2, "smile");
			}

			TileMap.AddLayer(layer);
		}

		public void Update() {

		}

		public void MoveAndSlide(Player player) {
			Collision.AABB playerCollider = player.Collider;

			//// Resolve if already colliding
			//Collision.CollisionResult? result = TileMap.Collide(playerCollider, player.z);

			//if (result is Collision.CollisionResult alreadyColliding) {
			//	Console.WriteLine("Colliding: Resolve");
			//	player.Pos += alreadyColliding.resolution;
			//	playerCollider = player.Collider;
			//}

			// Do sweep
			(Vec2 offset, Vec2 newVelocity) = TileMap.MoveAndSlide(playerCollider, player.Velocity, player.z, out List<Collision.SweepResult> collisions);

			player.Pos += offset;
			player.Velocity = newVelocity;

			Vec2 gravityVec = new(0f, gravity);
			// player.grounded = collision is Collision.SweepResult hit && hit.normal.Dot(gravityVec) < 0f;
			player.grounded = collisions.Any(hit => {
				return hit.normal.Dot(gravityVec) < 0f;
			});
		}

		public void Render(RenderContext ctx) {
			Backdrop.Render(ctx, this);

			int minX = (int)MathF.Floor(ctx.Camera.Left / 16) * 16;
			int minY = (int)MathF.Floor(ctx.Camera.Top / 16) * 16;
			int maxX = (int)MathF.Ceiling(ctx.Camera.Right / 16) * 16;
			int maxY = (int)MathF.Ceiling(ctx.Camera.Bottom / 16) * 16;

			for (int z = TileMap.LayerMin; z <= TileMap.LayerMax; z++) {
				for (int y = minY; y <= maxY; y += 16) {
					for (int x = minX; x <= maxX; x += 16) {
						foreach (Tile tile in TileMap.GetTilesByLayer(x / 16, y / 16, z)) {
							Texture texture = TextureRegistry.GetTexture(tile.texture);
							ctx.DrawTexture(texture, new(x, y));
						}
					}
				}
			}
		}

		public static void RegisterLevelTextures(RenderContext ctx) {
			Texture tree = Texture.LoadFromFile("res/textures/tree.png", ctx.Renderer);
			TextureRegistry.Register(tree, "tree");
			TextureRegistry.Register(Texture.LoadFromFile("res/textures/cloud.png", ctx.Renderer), "cloud");
		}
	}
}
