using SDL3;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal static class Common {
		// Return elapsed time (in seconds)
		public static double GetTime() {
			return (double)SDL.GetTicks() / 1000.0;
		}

		public static int GetFastSeed(params object?[] items) {
			if (items == null || items.Length == 0) return 0;
			HashCode hash = new HashCode();

			foreach (var item in items) {
				hash.Add(item?.GetHashCode() ?? 0);
			}

			return hash.ToHashCode();
		}

		public static SDL.Color CreateColor(byte r, byte g, byte b, byte a) {
			SDL.Color color = new() {
				R = r,
				G = g,
				B = b,
				A = a
			};
			return color;
		}

		// Alias for CreateColor
		public static SDL.Color RGB(byte r, byte g, byte b) {
			return CreateColor(r, g, b);
		}

		public static SDL.Color RGBA(byte r, byte g, byte b, byte a) {
			return CreateColor(r, g, b, a);
		}

		public static SDL.Color CreateColor(byte r, byte g, byte b) {
			return CreateColor(r, g, b, 255);
		}
	}
}
