using SDL3;
using System;
using System.Collections.Generic;
using System.Text;

namespace bario {
	internal class LevelBackdrop {
		private SDL.Color backgroundFill;
		private string? backgroundTileTexture = null;
		private List<BackgroundProp> props = [];
		private int seed;

		private Random random;

		public void Render(RenderContext ctx, Level level) {
			random = new Random(seed);

			ctx.ClearColor(backgroundFill);
			DrawPropsInRange(ctx);
		}

		private void DrawPropsInRange(RenderContext ctx) {
			foreach (BackgroundProp prop in props) {
				float start = MathF.Floor(ctx.Camera.Left / prop.frequency) * prop.frequency;
				float end = MathF.Ceiling(ctx.Camera.Right / prop.frequency) * prop.frequency;

				for (float x = start; x <= end; x += prop.frequency) {
					float y = MathF.Round((float)random.NextDouble() * (prop.yMax - prop.yMin) + prop.yMin);
					ctx.DrawTexture(prop.texture, new(x, y));
				}
			}
		}

		public class BackgroundProp {
			internal Texture texture;
			internal SDL.FRect txRect;
			internal float yMin;
			internal float yMax;
			internal float parallax = 1;

			// Appears every `frequency` units
			internal int frequency;

			public BackgroundProp(string textureName, float y, int frequency) {
				Texture texture = TextureRegistry.GetTexture(textureName);
				this.texture = texture;
				this.yMin = y;
				this.yMax = y;
				this.frequency = frequency;
			}

			public BackgroundProp(string textureName, Vec2 txPos, Vec2 txSize, float y, int frequency) {
				Texture texture = TextureRegistry.GetTexture(textureName);
				this.texture = texture;
				this.txRect = RenderContext.CreateFRect(txPos.x, txPos.y, txSize.x, txSize.y);
				this.yMin = y;
				this.yMax = y;
				this.frequency = frequency;
			}

			public BackgroundProp WithParallax(float parallax) {
				this.parallax = parallax;
				return this;
			}

			public BackgroundProp WithRandomYOffset(float offset) {
				yMax += offset / 2.0f;
				yMin -= offset / 2.0f;
				return this;
			}
		}

		public class BackdropBuilder {
			private LevelBackdrop backdrop;

			public BackdropBuilder() {
				backdrop = new LevelBackdrop() {
					backgroundFill = Common.RGB(0, 0, 0),
					backgroundTileTexture = null,
					props = [],
					seed = 0
				};
			}

			public BackdropBuilder WithBackgroundFill(SDL.Color color) {
				backdrop.backgroundFill = color;
				return this;
			}

			public BackdropBuilder WithBackgroundTexture(string texture) {
				backdrop.backgroundTileTexture = texture;
				return this;
			}

			public BackdropBuilder WithProp(BackgroundProp prop) {
				backdrop.props.Add(prop);
				return this;
			}
			
			public LevelBackdrop Build() {
				backdrop.seed = Common.GetFastSeed(backdrop);
				backdrop.random = new Random(backdrop.seed);

				return backdrop;
			}
		}
	}
}
