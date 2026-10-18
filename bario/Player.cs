using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal class Player {
		public string TextureSheet { get; private set; }

		public Vec2 Pos { get; set; }

		private int frame = 0;
		private int animationSpeed = 3;
		private int currentFrameTime = 0;

		public Vec2 Size { get; private set; }
		
		public static void RegisterTextures(RenderContext ctx) {
			Texture bario = Texture.LoadFromFile("res/textures/bario.png", ctx.Renderer);
			TextureRegistry.Register(bario, "bario");
		}

		public Player() {
			TextureSheet = "bario";
			Pos = new(0, 0);
			Size = new(16, 16);
		}

		public void Render(RenderContext ctx) {
			Texture texture = TextureRegistry.GetTexture(TextureSheet);
			ctx.DrawTexture(texture, new(16 * frame, 0), new(16, 18), new(0, 0));
		}

		public void Update(double dt) {
			currentFrameTime++;

			if (currentFrameTime >= animationSpeed) {
				frame++;
				if (frame >= 4) {
					frame = 0;
				}

				currentFrameTime = 0;
			}
		}
	}
}
