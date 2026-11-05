using bario.Tiles;
using Serilog.Debugging;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal class Player(Level level) {
		public const float GRAVITY_UP = 0.045f;
		public const float GRAVITY_DOWN = 0.085f;
		public const float MAX_FALL = 0.8f;
		public const float LOW_JUMP_EXTRA = 1.5f;
		public const float GROUND_ACCEL = 0.2f;
		public const float GROUND_FRICTION = 0.25f;
		public const float AIR_FRICTION = 0.01f;
		public const float AIR_ACCEL = 0.08f;
		public const float MAX_RUN_SPEED = 1.6f;
		public const int COYOTE_FRAMES = 6;

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

		public Vec2 Size { get; private set; } = new(16, 18);
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

		public static void RegisterTextures(RenderContext ctx) {
			Texture bario = Texture.LoadFromFile("res/textures/bario.png", ctx.Renderer);
			TextureRegistry.Register(bario, "bario");
		}

		public void Render(RenderContext ctx) {
			Texture texture = TextureRegistry.GetTexture(TextureSheet);
			ctx.DrawTextureFlipped(texture, new(16 * frame, 0), new(16, 18), Pos.Round(), !facing, false);
		}

		const float EPSILON = 0.001f;

		public void Update(Input input) {
			CheckJump(input);
			CheckWalk(input);

			// Gravity
			float gravity = Velocity.y < 0f ? GRAVITY_UP : GRAVITY_DOWN;
			Velocity += new Vec2(0f, MathF.Min(gravity, MAX_FALL));

			Level.MoveAndSlide(this);

			Animate();
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

		private void CheckJump(Input input) {
			if (grounded) {
				coyoteTimer = COYOTE_FRAMES;
			} else if (coyoteTimer > 0) {
				coyoteTimer--;
			}

			if (input.GetKeyJustPressed(SDL3.SDL.Keycode.C)) {
				jumpBuffer = JUMP_BUFFER_FRAMES;
			} else if (jumpBuffer > 0) {
				jumpBuffer--;
			}

			if (jumpBuffer > 0 && coyoteTimer > 0) {
				Velocity = new Vec2(Velocity.x, -JUMP_VELOCITY);
				
				jumpBuffer = 0;
				coyoteTimer = 0;
			}

			if (input.GetKeyJustReleased(SDL3.SDL.Keycode.C) && Velocity.y < -JUMP_CUT_VELOCITY) {
				Velocity = new Vec2(Velocity.x, -JUMP_CUT_VELOCITY);
			}
		}

		private void Animate() {
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
