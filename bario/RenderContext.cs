using SDL3;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal class RenderContext {
		private bool _disposed;

		public IntPtr Window { get; }
		public IntPtr Renderer { get; }
		public int Width { get; }
		public int Height { get; }
		public Camera Camera { get; }

		private RenderContext(IntPtr window, IntPtr renderer, int width, int height) {
			Window = window;
			Renderer = renderer;
			Width = width;
			Height = height;
			Camera = new Camera(new Vec2(0, 0), new Vec2(Game.FRAME_WIDTH, Game.FRAME_HEIGHT));
		}

		public static RenderContext Create(
			string appName,
			string appIdentifier,
			string windowTitle,
			int width,
			int height,
			SDL.InitFlags initFlags = SDL.InitFlags.Video,
			SDL.RendererLogicalPresentation presentation = SDL.RendererLogicalPresentation.Letterbox
		) {
			SDL.SetAppMetadata(appName, "1.0", appIdentifier);

			if (!SDL.Init(initFlags)) {
				throw new InvalidOperationException($"Couldn't initialize SDL: {SDL.GetError()}");
			}

			if (!SDL.CreateWindowAndRenderer(
				windowTitle,
				width,
				height,
				SDL.WindowFlags.Resizable,
				out var window,
				out var renderer
			)) {
				SDL.Quit();
				throw new InvalidOperationException($"Couldn't create window / renderer: {SDL.GetError()}");
			}

			SDL.SetRenderLogicalPresentation(renderer, width, height, presentation);
			SDL.SetRenderVSync(renderer, 1); // normal vsync

			TextureRegistry.Init(renderer);

			return new RenderContext(window, renderer, width, height);
		}

		public void Dispose() {
			if (_disposed) {
				return;
			}

			if (Renderer != IntPtr.Zero) {
				SDL.DestroyRenderer(Renderer);
			}

			if (Window != IntPtr.Zero) {
				SDL.DestroyWindow(Window);
			}

			SDL.Quit();
			_disposed = true;
		}

		private bool CheckStateValid() {
			if (_disposed) {
				Log.Error("Attempted to render after render context was disposed");
				return false;
			}

			return true;
		}

		public Vec2 ScreenToWorld(Vec2 screen) {
			return screen + Camera.origin;
		}

		public Vec2 WorldToScreen(Vec2 world) {
			return world - Camera.origin;
		}

		// Draw a texture in world coordinates
		public void DrawTexture(Texture texture, Vec2 pos) {
			if (!CheckStateValid()) { return; }

			Vec2 screen = WorldToScreen(pos);
			SDL.FRect dest = CreateFRect(screen.x, screen.y, texture.Width, texture.Height);

			SDL.RenderTexture(Renderer, texture.Handle, IntPtr.Zero, dest);
		}

		public void DrawTexture(Texture texture, Vec2 texPos, Vec2 texSize, Vec2 pos) {
			if (!CheckStateValid()) { return; }

			SDL.FRect src = CreateFRect(texPos.x, texPos.y, texSize.x, texSize.y);
			Vec2 screen = WorldToScreen(pos);
			SDL.FRect dest = CreateFRect(screen.x, screen.y, texSize.x, texSize.y);

			SDL.RenderTexture(Renderer, texture.Handle, src, dest);
		}

		public void ClearColor(SDL.Color color) {
			SDL.SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);
			SDL.RenderClear(Renderer);
		}

		public void ClearColor(byte r, byte g, byte b) {
			SDL.SetRenderDrawColor(Renderer, r, g, b, 255);
			SDL.RenderClear(Renderer);
		}

		public static SDL.Rect CreateRect(int x, int y, int w, int h) {
			return new SDL.Rect {
				X = x,
				Y = y,
				W = w,
				H = h
			};
		}

		public static SDL.FRect CreateFRect(float x, float y, float w, float h) {
			return new SDL.FRect {
				X = x,
				Y = y,
				W = w,
				H = h
			};
		}
	}
}
