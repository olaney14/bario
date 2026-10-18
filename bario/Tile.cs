using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal class Tile {
		public string texture = "error";
		public string RegistryName { get; private set; }
		public int RegistryID { get; private set; }

		public static readonly Tile EMPTY = new("error");

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
