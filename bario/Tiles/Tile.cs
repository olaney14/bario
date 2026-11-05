using System;
using System.Collections.Generic;
using System.Text;

namespace bario.Tiles {
	public class Tile {
		public string texture = "error";
		public string RegistryName { get; private set; }
		public int RegistryID { get; private set; }
		public bool solid = true;
		public float friction = 0.95f;

		public static readonly Tile EMPTY;

		static Tile() {
			EMPTY = new("error") {
				solid = false
			};
		}

		public Tile(string texture) {
			this.texture = texture;
			RegistryName = "error";
			RegistryID = 0;
		}

		public void SetRegistryValues(string name, int id) {
			RegistryName = name;
			RegistryID = id;
		}

		public Tile WithFriction(float friction) {
			this.friction = friction;
			return this;
		}

		public Tile WithSolid(bool solid) {
			this.solid = solid;
			return this;
		}
	}
}
