using System;

namespace Tetris.GA {
	public class Genome : TetrisPlayingANN {
		public Genome() : base() {

		}

		public Genome(double[] weights) : base(weights) {

		}

		public Genome Mutate() {

			double[] weights = new double[ann.Weights.Length];
			Array.Copy(ann.Weights, weights, weights.Length);
			for (int i = 0; i < weights.Length; i++) {
				if (Rand.NextDouble() > /*GASettings.MutateProbabilityInMutate*/ 0.5) {
					weights[i] = Rand.NextDouble() * (ANNSettings.MaxBoundary - ANNSettings.MinBoundary) + ANNSettings.MinBoundary;
				}
			}
			return new Genome(weights);
		}
	}
}
