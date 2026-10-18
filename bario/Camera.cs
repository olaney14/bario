using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal class Camera {
		public Vec2 origin;
		public Vec2 size;

		public Camera(Vec2 origin, Vec2 size) {
			this.origin = origin;
			this.size = size;
		}

		public void Update(Player player, Level level, double dt) {
			Vec2 goalPlayerScreen = size / 2;
			origin = player.Pos - goalPlayerScreen + player.Size / 2;
		}

		public float Top { get { return origin.y; } }
		public float Bottom { get { return origin.y + size.y; } }
		public float Left { get { return origin.x; } }
		public float Right { get { return origin.x + size.x; } }
	}
}
