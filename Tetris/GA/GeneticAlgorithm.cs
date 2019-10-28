using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using static Tetris.MainPage;

namespace Tetris.GA {

	/*
	 * Create the population
	 * Repeat:
	 *		Select
	 *		Crossover
	 *		Eliminate
	 *		Insert Selection
	 *		Mutate
	 */

	class GeneticAlgorithm : AIBase {

		private readonly List<Genome> population;
		private readonly Tuple<double, double>[] scores = new Tuple<double, double>[TetrisSettings.Runs];

		public GeneticAlgorithm(TrainingConsole console) : base(console) {

			population = new List<Genome>();
			console.WriteLn("Genetic Algorthim Initialized");
		}

		public void Begin() {
			List<string> history = new List<string>();
			Genome bestGenomeEver = null;
			console.WriteLn("Starting Genetic Algorithm", true);
			for (int r = 0; r < TetrisSettings.Runs; r++) {
				List<string> hist = new List<string>();
				double bestRunScore = 0;
				int evals = 0, g = 0;
				// Create the population
				population.Clear();
				Parallel.For(0, GASettings.InitialPopulation, i => {
					population.Add(new Genome());
					//console.WriteLn((++evals).ToString() + "," + population.Last().GetFitness().Item1);
					//history.Add((++evals).ToString() + "," + population.Last().GetFitness().Item1);
				});
				evals += GASettings.InitialPopulation;
				console.WriteLn("\nAll Genomes Initialized\n");
				hist.Add(evals.ToString() + "," + Select(GASettings.SelectionMethods.ABSOLUTE, true).GetFitness().Item1.ToString());

				//for (int g = 0; g < GASettings.Generations; g++) {
				bool run = true;
				while (run) {
					g++;
					Genome genome1 = Select(true);
					Genome genome2 = Select(true);
					while (genome1 == genome2) {
						genome2 = Select(true);
					}
					Tuple<Genome, Genome> noobs = Crossover(genome1, genome2);
					evals += 2;
					population.Add(noobs.Item1);
					population.Add(noobs.Item2);

					if (g % GASettings.AddEvery == 0) {
						population.Add(new Genome());
						evals++;
					}

					while (population.Count > GASettings.MaxPopulation) {
						population.Remove(Select(false));
					}

					if (TetrisSettings.LimitEvals) {
						Tuple<double, double> eh = Select(GASettings.SelectionMethods.ABSOLUTE).GetFitness();
						console.WriteLn(evals.ToString() + "," + eh.Item1.ToString());
						hist.Add(evals.ToString() + "," + eh.Item1.ToString());
						this.console.UpdateLn(evals.ToString() + "/" + TetrisSettings.Evals.ToString() + "   |   " + eh.Item1.ToString(), !TetrisSettings.Verbose);
					}
					else {
						console.WriteLn(Select(GASettings.SelectionMethods.ABSOLUTE).GetScore() + ", " + Select(GASettings.SelectionMethods.ABSOLUTE).GetFitness().Item2);
					}

					if ((TetrisSettings.LimitEvals && evals >= TetrisSettings.Evals) || (!TetrisSettings.LimitEvals && g >= GASettings.Generations)) {
						run = false;
						break;
					}

				}


				console.WriteLn("Done\n");
				foreach (Genome indiv in this.population) {
					console.WriteLn(indiv.GetScore().ToString());
				}
				console.WriteLn("");
				Genome best = Select(GASettings.SelectionMethods.ABSOLUTE);
				Tuple<double, double> score = best.Evaluate(true);
				if (score.Item1 > bestRunScore) {
					bestRunScore = score.Item1;
					history = hist;
					bestGenomeEver = best;
				}
				scores[r] = score;
				console.WriteLn(score.Item1.ToString(), true);
				//File.WriteAllText(TetrisSettings.SavePath, Select(GASettings.SelectionMethods.ABSOLUTE, true).ToString());
				console.WriteLn("Weights");
				console.WriteLn(best.ToString());
			}

			this.console.WriteLn("\n", true);

			foreach (string h in history) {
				this.console.WriteLn(h, true);
			}

			this.console.WriteLn("", true);
			this.console.WriteLn(bestGenomeEver.ToString(), true);


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

			console.WriteLn("Jobs Done", true);
		}

		private Genome Select(bool best = true) {
			if (best) {
				return Select(GASettings.SelectionMethod, true);
			}
			else {
				return Select(GASettings.EliminationMethod, false);
			}
		}

		private Genome Select(GASettings.SelectionMethods method, bool best = true) {
			switch (method) {
				case GASettings.SelectionMethods.ABSOLUTE:
					return best ?
						population.Aggregate((agg, next) =>
								(next.GetFitness().Item1 == agg.GetFitness().Item1 ? next.GetFitness().Item2 > agg.GetFitness().Item2 : next.GetFitness().Item1 > agg.GetFitness().Item1) ? next : agg) :
						population.Aggregate((agg, next) =>
								(next.GetFitness().Item1 == agg.GetFitness().Item1 ? next.GetFitness().Item2 < agg.GetFitness().Item2 : next.GetFitness().Item1 < agg.GetFitness().Item1) ? next : agg);
				case GASettings.SelectionMethods.TOURNAMENT:
					Genome[] genomes = new Genome[GASettings.K];
					for (int iK = 0; iK < genomes.Length; iK++) {
						Genome add = population[GetRandomInt(0, population.Count)];
						if (genomes.Contains(add)) {
							iK--;
						}
						else {
							genomes[iK] = add;
						}
					}
					return best ?
						genomes.Aggregate((agg, next) =>
								(next.GetFitness().Item1 == agg.GetFitness().Item1 ? next.GetFitness().Item2 > agg.GetFitness().Item2 : next.GetFitness().Item1 > agg.GetFitness().Item1) ? next : agg) :
						genomes.Aggregate((agg, next) =>
								(next.GetFitness().Item1 == agg.GetFitness().Item1 ? next.GetFitness().Item2 < agg.GetFitness().Item2 : next.GetFitness().Item1 < agg.GetFitness().Item1) ? next : agg);
				case GASettings.SelectionMethods.RANDOM:
					return population[(GetRandomInt(0, population.Count))];
				default:
					return null;
			}
		}

		private Tuple<Genome, Genome> Crossover(Genome genome1, Genome genome2) {
			MathNet.Numerics.Distributions.Normal normal = new MathNet.Numerics.Distributions.Normal(0, GASettings.MutStdDev);
			double[] one = new double[genome1.ann.Weights.Length], two = new double[genome2.ann.Weights.Length];
			for (int i = 0; i < one.Length; i++) {
				if (Rand.NextDouble() < GASettings.CrossProb) {
					one[i] = genome1.ann.Weights[i];
					two[i] = genome2.ann.Weights[i];
				}
				else {
					one[i] = genome2.ann.Weights[i];
					two[i] = genome1.ann.Weights[i];
				}

				if (Rand.NextDouble() < GASettings.MutProb) {
					one[i] += normal.Sample();
					one[i] = one[i] > ANNSettings.MaxBoundary ? ANNSettings.MaxBoundary : one[i];
					one[i] = one[i] < ANNSettings.MinBoundary ? ANNSettings.MinBoundary : one[i];
				}
				if (Rand.NextDouble() < GASettings.MutProb) {
					two[i] += normal.Sample();
					two[i] = two[i] > ANNSettings.MaxBoundary ? ANNSettings.MaxBoundary : two[i];
					two[i] = two[i] < ANNSettings.MinBoundary ? ANNSettings.MinBoundary : two[i];
				}
			}
			Genome genome3 = null, genome4 = null;
			Parallel.Invoke(() => {
				genome3 = new Genome(one);
			}, () => {
				genome4 = new Genome(two);
			});
			return Tuple.Create(genome3, genome4);
		}
	}
}
