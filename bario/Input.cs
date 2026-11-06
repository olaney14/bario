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

	public enum MouseButton {
		Left,
		Middle,
		Right
	}

	public class Input {
		private readonly Dictionary<SDL.Keycode, KeyState> keys = [];
		private readonly Dictionary<MouseButton, KeyState> buttons = [];
		private Vec2 mousePos = new(0f, 0f);
		private Vec2 delta = new(0f, 0f);
		private Vec2 scrollDelta = new(0f, 0f);

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

		public void MouseButtonPressed(MouseButton button) {
			KeyState state = GetMouseButtonState(button);

			if (state == KeyState.Released || state == KeyState.JustReleased) {
				buttons[button] = KeyState.JustPressed;
			}
		}

		public void MouseButtonReleased(MouseButton button) {
			KeyState state = GetMouseButtonState(button);

			if (state == KeyState.Pressed || state == KeyState.JustPressed) {
				buttons[button] = KeyState.JustReleased;
			}
		}

		public static MouseButton? FromSDLButtonIndex(uint index) {
			return index switch {
				SDL.ButtonLeft => (MouseButton?)MouseButton.Left,
				SDL.ButtonMiddle => (MouseButton?)MouseButton.Middle,
				SDL.ButtonRight => (MouseButton?)MouseButton.Right,
				_ => null,
			};
		}

		// Call at the end of frame
		public void Update() {
			foreach (SDL.Keycode key in keys.Keys.ToList()) {
				if (keys[key] == KeyState.JustReleased) {
					keys[key] = KeyState.Released;
				}

				if (keys[key] == KeyState.JustPressed) {
					keys[key] = KeyState.Pressed;
				}
			}

			foreach (MouseButton button in buttons.Keys.ToList()) {
				if (buttons[button] == KeyState.JustReleased) {
					buttons[button] = KeyState.Released;
				}

				if (buttons[button] == KeyState.JustPressed) {
					buttons[button] = KeyState.Pressed;
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

		public KeyState GetMouseButtonState(MouseButton button) {
			return buttons.GetValueOrDefault(button, KeyState.Released);
		}

		public bool GetMouseButtonPressed(MouseButton button) {
			KeyState state = GetMouseButtonState(button);
			return state == KeyState.JustPressed || state == KeyState.Pressed;
		}

		public bool GetMouseButtonJustPressed(MouseButton button) {
			return GetMouseButtonState(button) == KeyState.JustPressed;
		}

		public bool GetMouseButtonReleased(MouseButton button) {
			KeyState state = GetMouseButtonState(button);
			return state == KeyState.JustReleased || state == KeyState.Released;
		}

		public bool GetMouseButtonJustReleased(MouseButton button) {
			return GetMouseButtonState(button) == KeyState.JustReleased;
		}

		public Vec2 GetMovementVector() {
			float x = 0f;
			float y = 0f;

			// TODO: Movement keys interface
			if (GetKeyPressed(SDL.Keycode.Left) || GetKeyPressed(SDL.Keycode.A)) x -= 1f;
			if (GetKeyPressed(SDL.Keycode.Right) || GetKeyPressed(SDL.Keycode.D)) x += 1f;
			if (GetKeyPressed(SDL.Keycode.Up) || GetKeyPressed(SDL.Keycode.W)) y -= 1f;
			if (GetKeyPressed(SDL.Keycode.Down) || GetKeyPressed(SDL.Keycode.S)) y += 1f;

			return new(x, y);
		}

		public Vec2 GetMousePosition() {
			return mousePos;
		}

		public void MouseMoved(Vec2 newPosition, Vec2 delta) {
			mousePos = newPosition;
			this.delta = delta;
		}

		public float GetScroll() {
			return scrollDelta.y;
		}

		public void Scrolled(Vec2 scrollDelta) {
			this.scrollDelta = scrollDelta;
		}
	}
}
