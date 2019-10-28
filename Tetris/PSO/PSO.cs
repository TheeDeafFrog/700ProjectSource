using MathNet.Numerics.LinearAlgebra;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using static Tetris.MainPage;

namespace Tetris.PSO {
	class PSOActual : AIBase {
		//private readonly List<Particle> Particles = new List<Particle>();
		private readonly Particle[] Particles = new Particle[PSOSettings.Particles];

		public PSOActual(TrainingConsole console) : base(console) {

		}

		Tuple<double, double> bestScoreYet;
		Vector<double> bestPositionYet;

		public void Begin() {
			List<string> bestHist = new List<string>();
			double bestScoreOverRuns = 0;
			console.WriteLn("PSO Selected", true);
			for (int r = 0; r < TetrisSettings.Runs; r++) {
				List<string> hist = new List<string>();
				int evals = 0, iter = 0;
				bool run = true;

				this.bestScoreYet = new Tuple<double, double>(0, 0);
				console.WriteLn("Initilizing Population");
				Parallel.For(0, PSOSettings.Particles, i => {
					Particle particle = new Particle();
					this.console.WriteLn((++evals).ToString() + "," + particle.GetFitness().Item1);
					//Particles.Add(particle);
					Particles[i] = particle;
				});

				UpdateBest(evals, hist, TetrisSettings.LimitEvals ? evals : iter);

				console.WriteLn("\nFOR THE SWARM\n");
				switch (PSOSettings.NeighbourhoodTopology) {
					//case PSOSettings.NeighbourhoodTopologies.STAR:
					case PSOSettings.NeighbourhoodTopologies.RING:
						for (int i = 0; i < Particles.Length; i++) {
							Particle particle = Particles[i];
							particle.Neighbourhood.Add(i == Particles.Length - 1 ? Particles[0] : Particles[i + 1]); // Next
							particle.Neighbourhood.Add(i == 0 ? Particles[Particles.Length - 1] : Particles[i - 1]); // Previous
						}
						break;
					case PSOSettings.NeighbourhoodTopologies.STAR:
						foreach (Particle me in Particles) {
							foreach (Particle other in Particles) {
								if (other != me) {
									me.Neighbourhood.Add(other);
								}
							}
						}
						break;
				}

				while (run) {
					iter++;
				//for (int i = 0; i < PSOSettings.Iterations; i++) {
					foreach (Particle particle in this.Particles) {
						particle.DetermineNeighbourhoodBest();
					}
					Parallel.ForEach(Particles, (particle) => {
						particle.Move();
					});
					evals += Particles.Length;
					//foreach (Particle particle in Particles) {
					//	particle.Move();
					//}
					UpdateBest(evals, hist, TetrisSettings.LimitEvals ? evals : iter);
					//this.console.WriteLn(this.bestScoreYet.Item1.ToString());
					if ((TetrisSettings.LimitEvals && evals >= TetrisSettings.Evals) || (!TetrisSettings.LimitEvals && iter >= PSOSettings.Iterations)) {
						run = false;
						break;
					}

				}
				//this.console.WriteLn(this.bestScoreYet.Item1.ToString(), true);
				TetrisPlayingANN temp = new TetrisPlayingANN(this.bestPositionYet.ToArray(), false);
				this.console.WriteLn(temp.Evaluate(true).Item1.ToString(), true);
				if (TetrisSettings.Verbose) {
					this.console.WriteLn("Weights");
					foreach (double weight in bestPositionYet.ToArray()) {
						this.console.WriteLn(weight.ToString() + ",");
					}
				}
				if (this.bestScoreYet.Item1 > bestScoreOverRuns) {
					bestScoreOverRuns = this.bestScoreYet.Item1;
					bestHist = hist;
				}
			}
			this.console.WriteLn("\nJobs Done\n", true);

			foreach (string h in bestHist) {
				this.console.WriteLn(h, true);
			}

			ToastVisual visual = new ToastVisual() {
				BindingGeneric = new ToastBindingGeneric() {
					Children = {
						new AdaptiveText() {
							Text = "Your Majesty"
						},
						new AdaptiveText() {
							Text = "It is done"
						}
					}
				}
			};
			ToastContent toastContent = new ToastContent() {
				Visual = visual
			};
			var toast = new ToastNotification(toastContent.GetXml());
			ToastNotificationManager.CreateToastNotifier().Show(toast);
		}

		private void UpdateBest(int evals, List<string> hist, int it) {
			Particle bestNow =  Particles.Aggregate((agg, next) => (next.GetFitness().Item1 == agg.GetFitness().Item1 ? next.GetFitness().Item2 > agg.GetFitness().Item2 : next.GetFitness().Item1 > agg.GetFitness().Item1) ? next : agg);
			if (bestNow.GetFitness().Item1 > this.bestScoreYet.Item1) {
				this.bestScoreYet = bestNow.GetFitness();
				this.bestPositionYet = bestNow.Position;
			} else if (bestNow.GetFitness().Item1 == this.bestScoreYet.Item1 && bestNow.GetFitness().Item2 > this.bestScoreYet.Item2) {
				this.bestScoreYet = bestNow.GetFitness();
				this.bestPositionYet = bestNow.Position;
			}
			if (TetrisSettings.LimitEvals) {
				hist.Add(evals.ToString() + "," + bestNow.GetFitness().Item1.ToString() + "," + this.bestScoreYet.Item1);
				this.console.UpdateLn(it.ToString() + "/" + TetrisSettings.Evals.ToString() + "   |   " + hist[hist.Count - 1], !TetrisSettings.Verbose);
			}
			else {
				hist.Add(bestNow.GetFitness().Item1.ToString() + ", " + this.bestScoreYet.Item1);
				this.console.UpdateLn(it.ToString() + "/" + PSOSettings.Iterations + "   |   " + bestNow.GetFitness().Item1.ToString() + ", " + this.bestScoreYet.Item1, !TetrisSettings.Verbose);
			}
		}
	}
}
