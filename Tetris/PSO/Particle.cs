using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetris.PSO {
	class Particle : TetrisPlayingANN {
		public Vector<double> Position;
		Vector<double> PersonalBestPosition;
		Tuple<double, double> PersonalBestScore;
		Vector<double> NeighbourhoodBest;
		Vector<double> Velocity;
		public List<Particle> Neighbourhood = new List<Particle>();

		public Particle() : base() {
			Position = Vector<double>.Build.DenseOfArray(this.ann.Weights);
			Velocity = Vector<double>.Build.Dense(ANNSettings.Dimensions, 0);
			PersonalBestPosition = Position.Clone();
			this.PersonalBestScore = GetFitness();
		}

		public void Move() {
			this.Velocity = this.Velocity.Multiply(PSOSettings.VelocityWeight);

			Vector<double> cognitiveComponent = Vector<double>.Build.DenseOfArray(Rand.RandomWeights(0, 1));
			cognitiveComponent = cognitiveComponent.Multiply(PSOSettings.CognitaveCoefficient);
			cognitiveComponent = cognitiveComponent.PointwiseMultiply(PersonalBestPosition.Subtract(Position));

			Vector<double> socialComponent = Vector<double>.Build.DenseOfArray(Rand.RandomWeights(0, 1));
			socialComponent = socialComponent.Multiply(PSOSettings.SocialCoefficient);
			socialComponent = socialComponent.PointwiseMultiply(this.NeighbourhoodBest.Subtract(Position));

			this.Velocity = this.Velocity.Add(cognitiveComponent);
			this.Velocity = this.Velocity.Add(socialComponent);

			Position = Position.Add(this.Velocity);

			for (int i = 0; i < ANNSettings.Dimensions; i++) {
				if (this.Position[i] > ANNSettings.MaxBoundary) {
					this.Position[i] = ANNSettings.MaxBoundary;
					this.Velocity[i] = this.Velocity[i] * -1;
				}
				if (this.Position[i] < ANNSettings.MinBoundary) {
					this.Position[i] = ANNSettings.MinBoundary;
					this.Velocity[i] = this.Velocity[i] * -1;
				}
			}

			this.ann.Weights = Position.ToArray();
			Evaluate();

			Tuple<double, double> fit = GetFitness();
			if (fit.Item1 > PersonalBestScore.Item1) {
				this.PersonalBestPosition = this.Position.Clone();
				this.PersonalBestScore = fit;
			}
			else if (fit.Item1 == PersonalBestScore.Item1 && fit.Item2 > PersonalBestScore.Item2) {
				this.PersonalBestPosition = this.Position.Clone();
				this.PersonalBestScore = fit;
			}
		}

		public void DetermineNeighbourhoodBest() {
			this.NeighbourhoodBest = this.Neighbourhood.Aggregate((agg, next) => (next.PersonalBestScore.Item1 == agg.PersonalBestScore.Item1 ? next.PersonalBestScore.Item2 > agg.PersonalBestScore.Item2 : next.PersonalBestScore.Item1 > agg.PersonalBestScore.Item1) ? next : agg).PersonalBestPosition;
		}
	}
}
