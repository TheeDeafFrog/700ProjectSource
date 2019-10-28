using System;

namespace Tetris {

	public class ANN {
		public int Input { get; set; }
		public int Hidden { get; set; }
		public double[] Weights { get; set; }

		/*
		 * Lets take some time to discuss the nature of weights the array:
		 *		Lets use an example, the ANN has 3 inputs (+1 bias), 2 hidden (+1 bias), and 1 output:
		 *			0: I1 > H1
		 *			1: I1 > H2
		 *			2: I2 > H1
		 *			3: I2 > H2
		 *			4: I3 > H1
		 *			5: I3 > H2
		 *			6: IB > H1
		 *			7: IB > H2
		 *			8: H1 > O0
		 *			9: H2 > O0
		 *			10: HB > O0
		 */

		public ANN(int input, int hidden) {
			Weights = Rand.RandomWeights();
			Input = input;
			Hidden = hidden;
		}

		public ANN(double[] weights, int input, int hidden) {
			Weights = weights;
			Input = input;
			Hidden = hidden;
		}

		public void Set(double[] weights) {
			this.Weights = weights;
		}

		public double Evaluate(PlacementPackage placement) {
			placement.Normalize();
			double[] intermediateScores = new double[Hidden + 1];
			//for (int iIntermediateScore = 0; iIntermediateScore < intermediateScores.Length - 1; iIntermediateScore++) {
			//	intermediateScores[iIntermediateScore] = 0;
			//}
			intermediateScores[intermediateScores.Length - 1] = 1;
			for (int iInput = 0; iInput <= Input; iInput++) {  // the last gotten feature is the input bias
				double feature = placement.GetFeature(iInput);
				int index = iInput * Hidden;
				for (int iHidden = 0; iHidden < Hidden; iHidden++) {
					intermediateScores[iHidden] += feature * Weights[index + iHidden];
				}
			}
			for (int inter = 0; inter < intermediateScores.Length - 1; inter++) {
				if (intermediateScores[inter] < 0) {
					intermediateScores[inter] = 0;
				}
			}
			double result = 0;
			int outputIndex = (this.Input + 1) * this.Hidden;
			for (int inter = 0; inter < intermediateScores.Length; inter++) {
				double temp = intermediateScores[inter] * Weights[outputIndex + inter];
				result += temp;
			}
			result = 1 / (1 + Math.Pow(Math.E, -result));
			return result;

			//double result = 0;
			//for (int i = 0; i < ANNSettings.Input; i++) {
			//	result += placement.GetFeature(i) * Weights[i];
			//}
			//return result;
		}
	}

	public class TetrisPlayingANN {
		public readonly TetrisGame tetrisGame;
		public readonly ANN ann;

		private double cleared, placed;

		public TetrisPlayingANN(TetrisGame tetrisGame, ANN ann) {
			this.tetrisGame = tetrisGame;
			this.ann = ann;
			this.Evaluate();
		}

		public TetrisPlayingANN(bool eval = true) {
			this.tetrisGame = new TetrisGame(TetrisSettings.Width, TetrisSettings.Height);
			this.ann = new ANN(ANNSettings.Input, ANNSettings.Hidden);
			if (eval) {
				this.Evaluate();
			}
		}

		public TetrisPlayingANN(double[] weights, bool eval = true) {
			this.tetrisGame = new TetrisGame(TetrisSettings.Width, TetrisSettings.Height);
			this.ann = new ANN(weights, ANNSettings.Input, ANNSettings.Hidden);
			if (eval) {
				this.Evaluate();
			}
		}

		public Tuple<double, double> Evaluate(bool full = false) {
			int evalGames = full ? TetrisSettings.EvalutaionGames : TetrisSettings.TrainEvaluationGames;
			double[] _clearedRows = new double[evalGames];
			double[] _placedPieces = new double[evalGames];
			for (int i = 0; i < evalGames; i++) {
				if (full) {
					this.tetrisGame.ResetGame(TetrisSettings.Width, TetrisSettings.Height);
				}
				else {
					this.tetrisGame.ResetGame(TetrisSettings.TrainWidth, TetrisSettings.TrainHeight);
				}
				PlacementPackage[] packages = tetrisGame.GetPackages();
				while (packages.Length > 0) {
					PlacementPackage bestPackage = packages[0];
					bestPackage.PackageScore = ann.Evaluate(bestPackage);
					for (int iPackage = 1; iPackage < packages.Length; iPackage++) {
						PlacementPackage package = packages[iPackage];
						package.PackageScore = ann.Evaluate(package);
						if (package.PackageScore > bestPackage.PackageScore) {
							bestPackage = package;
						}
					}
					tetrisGame.ApplyPackage(bestPackage);
					packages = tetrisGame.GetPackages();
				}
				_clearedRows[i] = tetrisGame.Score;
				_placedPieces[i] = tetrisGame.PlacedPieces;
			}
			double totCleared = 0, totPlaced = 0;
			for (int i = 0; i < evalGames; i++) {
				totCleared += _clearedRows[i];
				totPlaced += _placedPieces[i];
			}
			this.cleared = totCleared / evalGames;
			this.placed = placed / evalGames;
			return new Tuple<double, double>(cleared, placed);
		}

		public double GetScore(bool reEvaluate = false) {
			return GetFitness(reEvaluate).Item1;
		}

		public Tuple<double, double> GetFitness(Boolean reEvaluate = false) {
			if (reEvaluate) {
				Evaluate();
			}
			return new Tuple<double, double>(cleared, placed);
		}

		override public string ToString() {
			string res = "";
			foreach (double weight in this.ann.Weights) {
				res += weight.ToString() + ",\n";
			}
			return res;
		}
	}
}
