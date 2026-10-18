using SDL3;
using Serilog;

namespace bario {
	internal static class Program {
		public const string APP_NAME = "bario";
		public const string APP_IDENTIFIER = "com.olaney.bario";
		public const string WINDOW_TITLE = "bario";
		public const int DEFAULT_WINDOW_WIDTH = Game.FRAME_WIDTH * 4;
		public const int DEFAULT_WINDOW_HEIGHT = Game.FRAME_HEIGHT * 4;

		private const double PHYSICS_STEP = 1.0 / 60.0;
		private const double MAX_FRAME_TIME = 0.25;

		[STAThread]
		private static int Main(string[] args) {
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.CreateLogger();

			RenderContext? ctx = null;
			
			try {
				ctx = RenderContext.Create(
					APP_NAME, 
					APP_IDENTIFIER, 
					WINDOW_TITLE,
					DEFAULT_WINDOW_WIDTH, 
					DEFAULT_WINDOW_HEIGHT
				);

				Game game = new(ctx);

				double previousTime = Common.GetTime();
				double accumulator = 0.0;

				while (!game.shouldExit) {
					while (SDL.PollEvent(out var sdlEvent)) {
						if (sdlEvent.Type == (uint)SDL.EventType.Quit) {
							return 0;
						}
					}

					// Separate update and render step
					double currentTime = Common.GetTime();
					double frameTime = Math.Min(currentTime - previousTime, MAX_FRAME_TIME);

					previousTime = currentTime;
					accumulator += frameTime;

					// Events
					while (accumulator >= PHYSICS_STEP) {
						game.Update(PHYSICS_STEP);
						accumulator -= PHYSICS_STEP;
					}

					game.Render();
				}
			
				return 0;
			} catch (Exception ex) {
				SDL.LogError(SDL.LogCategory.Application, ex.Message);
				return 1;
			} finally {
				ctx?.Dispose();
				Log.CloseAndFlush();
			}
		}
	}
}
