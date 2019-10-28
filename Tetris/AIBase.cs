using System;
using static Tetris.MainPage;

namespace Tetris {
	public class AIBase {

		protected readonly TrainingConsole console;

		public AIBase(TrainingConsole console) {
			this.console = console;
		}

		//public static double GetRandomDouble(double minimum, double maximum) {
		//	return Rand.NextDouble() * (maximum - minimum) + minimum;
		//}

		public static int GetRandomInt(int minimum, int maximum) {
			return Rand.Next(maximum - minimum) + minimum;
		}
	}
}
