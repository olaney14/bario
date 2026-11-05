using System;
using System.Collections.Generic;
using System.Text;

namespace bario.Tiles {
	internal class Tile {
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
	}
}
