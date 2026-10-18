using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal static class TextureRegistry {
		private static readonly Dictionary<string, Texture> textures = [];

		public static void Init(IntPtr renderer) {
			Register(Texture.CreateDefaultTexture(16, 16, renderer), "error");
		}

		public static void Register(Texture texture, string name) {
			if (textures.ContainsKey(name)) {
				Log.Warning("Duplicate texture registry on `{A}`; overwriting", name);
			}

			textures[name] = texture;
		}

		public static Texture GetTexture(string name) {
			if (!textures.ContainsKey(name)) {
				Log.Warning("Texture {A} not present", name);
				return textures["error"];
			}
			return textures[name];
		}

		public static bool KeyPresent(string name) {
			return textures.ContainsKey(name);
		}
	}
}
