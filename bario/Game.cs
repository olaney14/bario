using SDL3;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal class Game {
		public const int FRAME_WIDTH = 320;
		public const int FRAME_HEIGHT = 180;

		public Level Level { get; private set; }
		public Player Player { get; private set; }

		private readonly RenderContext ctx;

		public bool shouldExit = false;

		private IntPtr renderTexture;

		public Game(RenderContext ctx) {
			this.ctx = ctx;

			Player.RegisterTextures(ctx);
			Level.RegisterLevelTextures(ctx);

			renderTexture = SDL.CreateTexture(
				ctx.Renderer,
				SDL.PixelFormat.RGBA8888,
				SDL.TextureAccess.Target,
				FRAME_WIDTH,
				FRAME_HEIGHT
			);
			SDL.SetTextureScaleMode(renderTexture, SDL.ScaleMode.PixelArt);
			SDL.GetTextureSize(renderTexture, out float w, out float h);

			Player = new Player();
			Level = new Level(16);
		}

		public void Update(double dt) {
			Player.Update(dt);
			ctx.Camera.Update(Player, Level, dt);
		}

		public void Render() {
			// Render to lower-res texture
			SDL.SetRenderTarget(ctx.Renderer, renderTexture);

			SDL.SetRenderDrawColor(ctx.Renderer, 0, 0, 0, 255);
			SDL.RenderClear(ctx.Renderer);

			Level.Render(ctx);
			Player.Render(ctx);

			// Render scaled texture to window
			SDL.SetRenderTarget(ctx.Renderer, IntPtr.Zero);
			SDL.RenderClear(ctx.Renderer);

			SDL.RenderTexture(ctx.Renderer, renderTexture, IntPtr.Zero, IntPtr.Zero);

			SDL.RenderPresent(ctx.Renderer);
		}
	}
}
