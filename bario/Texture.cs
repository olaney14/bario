using SDL3;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	public class Texture {
		private bool _disposed;

		public IntPtr Handle { get; }
		public int Width { get; }
		public int Height { get; }

		private Texture(IntPtr handle, int width, int height) {
			Handle = handle;
			Width = width;
			Height = height;
		}

		// Attempt to load a texture from file
		public static Texture LoadFromFile(string path, IntPtr renderer) {
			IntPtr surface = Image.Load(path);

			if (surface == IntPtr.Zero) {
				Log.Warning("Error creating surface: {A}", SDL.GetError());
				// Console.WriteLine($"[WARNING] Error creating surface: {SDL.GetError()}");
				return CreateDefaultTexture(16, 16, renderer);
			}

			IntPtr texture = SDL.CreateTextureFromSurface(renderer, surface);

			if (texture == IntPtr.Zero) {
				Log.Warning("Error creating texture: {A}", SDL.GetError());
				// Console.WriteLine($"[WARNING] Error creating texture: {SDL.GetError()}");
				return CreateDefaultTexture(16, 16, renderer);
			}

			SDL.SetTextureScaleMode(texture, SDL.ScaleMode.PixelArt);

			SDL.GetTextureSize(texture, out float width, out float height);
			return new Texture(texture, (int)width, (int)height);
		}
	
		public static unsafe Texture FromPixels(IntPtr renderer, uint[] pixels, int width, int height, SDL3.SDL.PixelFormat format) {
			int pitch = width * sizeof(uint); // ????
			IntPtr surface = IntPtr.Zero;
			IntPtr texture = IntPtr.Zero;

			// Pin 
			// Stops GC from moving it around
			fixed (uint* pixelsPtr = pixels) {
				surface = SDL3.SDL.CreateSurfaceFrom(
					width,
					height,
					format,
					(IntPtr)pixelsPtr,
					pitch
				);

				if (surface == IntPtr.Zero) {
					throw new Exception($"Failed to create surface: {SDL.GetError()}");
				}

				texture = SDL.CreateTextureFromSurface(renderer, surface);
				SDL.DestroySurface(surface);
			}
			// Unpin
			// We copied over texture data already, GC can do what it likes

			if (texture == IntPtr.Zero) {
				throw new Exception($"Failed to create texture: {SDL.GetError()}");
			}

			return new Texture(texture, width, height);
		}

		private const int DEFAULT_TEXTURE_BLOCK_SIZE = 2;

		public static Texture CreateDefaultTexture(int width, int height, IntPtr renderer) {
			uint[] rawPixels = new uint[width * height];
			uint currentColor = 0xFF000000;

			for (int blockY = 0; blockY < height / DEFAULT_TEXTURE_BLOCK_SIZE; blockY++) {
				for (int blockX = 0; blockX < width / DEFAULT_TEXTURE_BLOCK_SIZE; blockX++) {
					int x = blockX * DEFAULT_TEXTURE_BLOCK_SIZE;
					int y = blockY * DEFAULT_TEXTURE_BLOCK_SIZE;
					int xEnd = Math.Min((x + DEFAULT_TEXTURE_BLOCK_SIZE), width);
					int yEnd = Math.Min((y + DEFAULT_TEXTURE_BLOCK_SIZE), height);

					for (int j = y; j < yEnd; j++) {
						for (int i = x; i < xEnd; i++) {
							rawPixels[j * width + i] = currentColor;
						}
					}

					if (currentColor == 0xFF000000) {
						currentColor = 0xFF7F007F; // purple
					} else {
						currentColor = 0xFF000000;
					}
				}

				if (currentColor == 0xFF000000) {
					currentColor = 0xFF7F007F; // purple
				} else {
					currentColor = 0xFF000000;
				}
			}

			return FromPixels(renderer, rawPixels, width, height, SDL.PixelFormat.ARGB8888);
		}
	}
}
