using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	public struct Vec2 {
		public float x;
		public float y;

		public Vec2(float x, float y) {
			this.x = x;
			this.y = y;
		}

		public Vec2(float both) {
			this.x = both;
			this.y = both;
		}

		public static Vec2 operator +(Vec2 lhs, Vec2 rhs) {
			return new Vec2(lhs.x + rhs.x, lhs.y + rhs.y);
		}

		public static Vec2 operator -(Vec2 v) {
			return new Vec2(-v.x, -v.y);
		}

		public static Vec2 operator -(Vec2 rhs, Vec2 lhs) {
			return new Vec2(rhs.x - lhs.x, rhs.y - lhs.y);
		}

		public static Vec2 operator *(Vec2 v, float scalar) { 
			return new Vec2(scalar * v.x, scalar * v.y);
		}

		public static Vec2 operator *(float scalar, Vec2 v) {
			return new Vec2(scalar * v.x, scalar * v.y);
		}

		public static Vec2 operator *(Vec2 lhs, Vec2 rhs) {
			return new Vec2(lhs.x * rhs.x, lhs.y * rhs.y);
		}

		public static Vec2 operator /(Vec2 v, float scalar) {
			return new Vec2(v.x / scalar, v.y / scalar);
		}

		public float Dot(Vec2 v) {
			return x * v.x + y * v.y;
		}

		public Vec2 Abs() {
			return new Vec2(MathF.Abs(x), MathF.Abs(y));
		}

		public Vec2 Round() {
			return new(MathF.Round(x), MathF.Round(y));
		}

		public Vec2 Floor() {
			return new(MathF.Floor(x), MathF.Floor(y));
		}

		public Vec2 Ceiling() {
			return new(MathF.Ceiling(x), MathF.Ceiling(y));
		}

		public float Magnitude() {
			return MathF.Sqrt(x * x + y * y);
		}

		public float MagnitudeSquared() {
			return x * x + y * y;
		}

		public float LengthSquared() {
			return MagnitudeSquared();
		}

		public float Length() {
			return Magnitude();
		}

		public Vec2 Unit() {
			return this / Magnitude();
		}

		public Vec2 Normalize() {
			return Unit();
		}

		public override string ToString() {
			return "(" + x + ", " + y + ")";
		}
	}
}
