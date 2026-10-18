using SDL3;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	public enum KeyState {
		Pressed,
		Released,
		JustPressed,
		JustReleased
	}

	public class Input {
		private readonly Dictionary<SDL.Keycode, KeyState> keys = [];

		public void KeyPressed(SDL.Keycode key) {
			KeyState state = GetKeyState(key);

			// Ignore repeat keydown events
			if (state == KeyState.Released || state == KeyState.JustReleased) {
				keys[key] = KeyState.JustPressed;
			}
		}

		public void KeyReleased(SDL.Keycode key) {
			KeyState state = GetKeyState(key);

			// Ignore repeat keyup events
			if (state == KeyState.Pressed || state == KeyState.JustPressed) {
				keys[key] = KeyState.JustReleased;
			}
		}

		// Call at the end of frame
		public void Update() {
			var keycodes = keys.Keys.ToList();
			for (int i = 0; i < keycodes.Count; i++) {
				if (keys[keycodes[i]] == KeyState.JustReleased) {
					keys[keycodes[i]] = KeyState.Released;
				}

				if (keys[keycodes[i]] == KeyState.JustPressed) {
					keys[keycodes[i]] = KeyState.Pressed;
				}
			}
		}

		public KeyState GetKeyState(SDL.Keycode key) {
			return keys.GetValueOrDefault(key, KeyState.Released);
		}

		public bool GetKeyPressed(SDL.Keycode key) {
			KeyState state = GetKeyState(key);
			return state == KeyState.JustPressed || state == KeyState.Pressed;
		}

		public bool GetKeyJustPressed(SDL.Keycode key) {
			return GetKeyState(key) == KeyState.JustPressed;
		}

		public bool GetKeyReleased(SDL.Keycode key) {
			KeyState state = GetKeyState(key);
			return state == KeyState.Released || state == KeyState.JustReleased;
		}

		public bool GetKeyJustReleased(SDL.Keycode key) {
			return GetKeyState(key) == KeyState.JustReleased;
		}
	}
}
