using System;
using System.ComponentModel;
using System.Threading;
using Tetris.GA;
using Tetris.PSO;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tetris {
	public sealed partial class MainPage : Page, INotifyPropertyChanged {

		private readonly TetrisInput parameters = new TetrisInput();

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		private readonly TrainingConsole console;

		public MainPage() {
			this.InitializeComponent();
			this.console = new TrainingConsole(outputConsole);
		}

		private void startGame(object sender, RoutedEventArgs e) {
			Frame.Navigate(typeof(Tetris), this.parameters);
		}

		public class TrainingConsole {

			private readonly TextBlock console;
			private bool inUpdate = false;
			private string oldText = "";
			public TrainingConsole(TextBlock console) {
				this.console = console;
				console.Text = "";
			}

			public void WriteLn(string writeLine, bool print = TetrisSettings.Verbose) {
				if (print) {
					_ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
						if (!inUpdate) {
							console.Text += writeLine + "\n";
						}
						else {
							inUpdate = false;
							console.Text = oldText + writeLine + "\n";
						}
					});
				}
			}

			public void UpdateLn(string writeLine, bool print = TetrisSettings.Verbose) {
				if (print) {
					_ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
						if (!inUpdate) {
							oldText = console.Text;
							console.Text += writeLine + "\n";
							inUpdate = true;
						}
						else {
							console.Text = oldText + writeLine + "\n";
						}
					});
				}
			}
		}

		private void TrainGA(object sender, RoutedEventArgs e) {
			GeneticAlgorithm geneticAlgorithm = new GeneticAlgorithm(console);
			Thread trainingThread = new Thread(() => geneticAlgorithm.Begin());
			trainingThread.Start();
		}

		private void TrainPSO(object sender, RoutedEventArgs e) {
			PSOActual pso = new PSOActual(console);
			Thread trainingThread = new Thread(() => pso.Begin());
			trainingThread.Start();
		}
	}
}
