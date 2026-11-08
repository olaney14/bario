using bario.Tiles;
using Serilog.Debugging;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	public class Player(Level level) {
		public const float GRAVITY_UP = 0.08f;
		public const float GRAVITY_DOWN = 0.14f;
		public const float MAX_FALL = 1.4f;
		public const float LOW_JUMP_EXTRA = 1.5f;
		public const float GROUND_ACCEL = 0.2f;
		public const float GROUND_FRICTION = 0.25f;
		public const float AIR_FRICTION = 0.01f;
		public const float AIR_ACCEL = 0.12f;
		public const float MAX_RUN_SPEED = 1.6f;
		public const int COYOTE_FRAMES = 6;

		public const float LEDGE_GRAB_RANGE = 4f;
		public static readonly float LEDGE_JUMP_VELOCITY = JumpVelocityForHeight(JUMP_HEIGHT_MAX * 0.8f, GRAVITY_UP);

		private static float JumpVelocityForHeight(float height, float gravity) {
			return MathF.Sqrt(2f * gravity * height);
		}

		public const float JUMP_HEIGHT_MAX = 2.5f * 16f;
		public const float JUMP_HEIGHT_MIN = 0.8f * 16f;
		public static readonly float JUMP_VELOCITY = JumpVelocityForHeight(JUMP_HEIGHT_MAX, GRAVITY_UP);
		public static readonly float JUMP_CUT_VELOCITY = JumpVelocityForHeight(JUMP_HEIGHT_MIN, GRAVITY_UP);
		public const int JUMP_BUFFER_FRAMES = 8;

		public string TextureSheet { get; private set; } = "bario";

		public Vec2 Pos { get; set; } = new(0, 0);
		public Vec2 Velocity { get; set; }

		private int frame = 0;
		private int animationSpeed = 3;
		private int currentFrameTime = 0;
		private int coyoteTimer = 0;
		private int jumpBuffer = 0;

		private bool grabbingLedge = false;
		private int ledgeTileX = 0;
		private int ledgeTileY = 0;
		private float ledgeSnapX = 0f;
		private int ledgeFaceDir = 0;

		public Vec2 Size { get; private set; } = new(12, 14);
		public Vec2 TextureOffset { get; set; } = new(-2f, -4f);
		public Vec2 Center { get { return Pos + Size / 2f; } }
		public Collision.AABB Collider {
			get {
				return new() { min = Pos, max = Pos + Size };
			}
		}
		public Level Level { get; private set; } = level;
		public bool grounded = false;
		public int z = 0;
		public bool facing = true; // True : Left, False : Right
		public bool editorControls = false;

		public static void RegisterTextures(RenderContext ctx) {
			Texture bario = Texture.LoadFromFile("res/textures/bario.png", ctx.Renderer);
			TextureRegistry.Register(bario, "bario");
		}

		public void Render(RenderContext ctx) {
			Texture texture = TextureRegistry.GetTexture(TextureSheet);
			ctx.DrawTextureFlipped(texture, new(16 * frame, 0), new(16, 18), Pos.Round() + TextureOffset, !facing, false);
		}

		const float EPSILON = 0.001f;

		public void Update(Input input) {
			if (editorControls) {
				EditorControls(input);
				return;
			}
			
			CheckJump(input);
			CheckWalk(input);
			CheckLedge(input);

			// Gravity
			if (!grabbingLedge) {
				float gravity = Velocity.y < 0f ? GRAVITY_UP : GRAVITY_DOWN;
				Velocity += new Vec2(0f, MathF.Min(gravity, MAX_FALL));
			}

			Level.MoveAndSlide(this);

			Animate();
		}

		private void EditorControls(Input input) {
			Vec2 move = input.GetMovementVector();
			Pos += move;
		}

		private void CheckWalk(Input input) {
			Vec2 inputVec = input.GetMovementVector();
			if (inputVec.x > 0f && Velocity.x >= 0f) {
				facing = true;
			} else if (inputVec.x < 0f && Velocity.x <= 0f) {
				facing = false;
			}

			float targetX = inputVec.x * MAX_RUN_SPEED;
			float accel;

			if (grounded) {
				accel = MathF.Abs(targetX) > EPSILON ? GROUND_ACCEL : GROUND_FRICTION;
			} else {
				accel = MathF.Abs(targetX) > EPSILON ? AIR_ACCEL : AIR_FRICTION;
			}

			Velocity = new Vec2(Common.MoveToward(Velocity.x, targetX, accel), Velocity.y);
		}

		private bool TriggeredJump(Input input) {
			return input.GetKeyJustPressed(SDL3.SDL.Keycode.C) || input.GetKeyJustPressed(SDL3.SDL.Keycode.Up) || input.GetKeyJustPressed(SDL3.SDL.Keycode.Space);
		}

		private bool ReleasedJump(Input input) {
			return input.GetKeyJustReleased(SDL3.SDL.Keycode.C) || input.GetKeyJustReleased(SDL3.SDL.Keycode.Up) || input.GetKeyJustReleased(SDL3.SDL.Keycode.Space);
		}

		private void CheckJump(Input input) {
			if (grounded) {
				coyoteTimer = COYOTE_FRAMES;
			} else if (coyoteTimer > 0) {
				coyoteTimer--;
			}

			if (TriggeredJump(input)) {
				jumpBuffer = JUMP_BUFFER_FRAMES;
			} else if (jumpBuffer > 0) {
				jumpBuffer--;
			}

			if (jumpBuffer > 0 && coyoteTimer > 0) {
				Velocity = new Vec2(Velocity.x, -JUMP_VELOCITY);
				
				jumpBuffer = 0;
				coyoteTimer = 0;
			}

			if (ReleasedJump(input) && Velocity.y < -JUMP_CUT_VELOCITY) {
				Velocity = new Vec2(Velocity.x, -JUMP_CUT_VELOCITY);
			}
		}

		private bool NearLedge() {
			if (grounded) return false;
			if (MathF.Abs(Velocity.x) < EPSILON) return false;

			int dir = Velocity.x > 0f ? 1 : -1;

			float wallEdge = dir > 0 ? Collider.max.x : Collider.min.x;
			int wallTileX = (int)MathF.Floor((wallEdge + dir * 0.5f) / 16f);

			float handY = Pos.y + Size.y * 0.5f;
			int handTileY = (int)MathF.Floor(handY / 16f);
			int topTileY = (int)MathF.Floor(Pos.y / 16f);

			for (int tileY = handTileY; tileY >= topTileY; tileY--) {
				bool solidHere = Level.TileMap.GetTileSolid(wallTileX, tileY);
				bool solidAbove = Level.TileMap.GetTileSolid(wallTileX, tileY - 1);

				if (!solidHere || solidAbove) continue;

				float tileNearEdge = dir > 0 ? wallTileX * 16f : (wallTileX + 1) * 16f;

				float dist = MathF.Abs(wallEdge - tileNearEdge);

				if (dist > LEDGE_GRAB_RANGE) continue;

				float tileTopY = tileY * 16f;
				float tileCenterY = tileTopY + 8f;
				if (handY < tileTopY || handY > tileCenterY) continue;

				ledgeTileX = wallTileX;
				ledgeTileY = tileY;
				ledgeSnapX = tileNearEdge;
				ledgeFaceDir = dir;
				return true;
			}

			return false;
		}

		private void CheckLedge(Input input) {
			if (grabbingLedge) {
				UpdateLedgeGrab(input);
				return;
			}

			// Corner vault
			bool nearLedge = NearLedge();
			if (Velocity.y < 0f && nearLedge && jumpBuffer > 0) {
				Velocity = new Vec2(ledgeFaceDir * MAX_RUN_SPEED, -LEDGE_JUMP_VELOCITY);
				jumpBuffer = 0;
				coyoteTimer = 0;
				return;
			}

			// Ledge grab
			if (nearLedge && Velocity.y >= 0f) {
				AttachToLedge();
			}
		}

		private void AttachToLedge() {
			grabbingLedge = true;

			float newX = ledgeFaceDir > 0 ?
				ledgeSnapX - Size.x :
				ledgeSnapX;

			float newY = ledgeTileY * 16f - Size.y * 0.5f;

			Pos = new Vec2(newX, newY);
			Velocity = new Vec2(0f, 0f);

			coyoteTimer = 0;
			// jumpBuffer = 0;
		}

		private void UpdateLedgeGrab(Input input) {
			Velocity = new Vec2(0f, 0f);

			Vec2 inputVec = input.GetMovementVector();
			bool pushingAway = (ledgeFaceDir > 0 && inputVec.x < -EPSILON) ||
							   (ledgeFaceDir < 0 && inputVec.x > EPSILON);

			if (pushingAway) {
				ReleaseLedge();
				return;
			}

			if (jumpBuffer > 0) {
				ReleaseLedge();
				Velocity = new Vec2(0f, -LEDGE_JUMP_VELOCITY);
				coyoteTimer = 0;
				jumpBuffer = 0;
			}
		}

		private void ReleaseLedge() {
			grabbingLedge = false;
			coyoteTimer = COYOTE_FRAMES;
		}

		private void Animate() {
			if (grabbingLedge) {
				frame = 4;
				currentFrameTime = 0;
				return;
			}

			if (grounded) {
				if (MathF.Abs(Velocity.x) > 0.2f) {
					// Higher velocity = fewer game frames per animation frame (faster cycling)
					// Clamp to at least 1 to avoid division by zero
					float speed = MathF.Abs(Velocity.x);
					animationSpeed = (int)MathF.Max(1f, 8f / (speed));

					currentFrameTime++;

					if (currentFrameTime >= animationSpeed) {
						currentFrameTime = 0;
						frame = (frame + 1) % 4;
					}
				} else {
					// Standing still — reset to idle stance
					frame = 0;
					currentFrameTime = 0; // ← reset so run cycle starts cleanly
				}
			} else {
				frame = 1;
			}
		}
	}
}
