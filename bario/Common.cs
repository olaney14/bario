using SDL3;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

		public static void FillMatrix<T>(ref T[,] matrix, T value) {
			Span<T> flatSpan = MemoryMarshal.CreateSpan(ref matrix[0, 0], matrix.Length);
			flatSpan.Fill(value);
		}

		public static float MoveToward(float current, float target, float maxDelta) {
			float diff = target - current;
			if (MathF.Abs(diff) <= maxDelta) return target;
			return current + MathF.Sign(diff) * maxDelta;
		}
	}
}
