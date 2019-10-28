using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Tetris {
	public sealed partial class Tetris : Page {

		bool[,] boardOccupation;
		Rectangle[,] boardFills;

		private int width, height;
		private Piece currentPiece;

		PlacementPackage package;

		private TetrisGame game;

		private ANN ann;

		SolidColorBrush transparentBrush = new SolidColorBrush(Colors.Transparent);

		public Tetris() {
			this.InitializeComponent();
			this.currentPiece = new Piece();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			var parameters = (TetrisInput)e.Parameter;

			generateGrid(TetrisSettings.Width, TetrisSettings.Height);
			this.boardOccupation = new bool[width, height];
			this.boardFills = new Rectangle[width, height];
			this.game = new TetrisGame(width, height);

			this.ann = new ANN(ANNSettings.Weights, ANNSettings.Input, ANNSettings.Hidden);
			this.package = new PlacementPackage() {
				RemovedRows = 0
			};

			SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);

			for (int x = 0; x < this.width; x++) {
				for (int y = 0; y < this.height; y++) {
					var block = new Rectangle();
					block.Fill = transparentBrush;
					block.Stroke = blackBrush;
					Grid.SetColumn(block, x);
					Grid.SetRow(block, y);
					board.Children.Add(block);
					boardFills[x, y] = block;
				}
			}
		}
		
		private bool PlacePiece(Piece piece, int[] origin, int orientation) {
			int[,] toOccupy = piece.DetermineCellsToOccupy(origin, orientation);
			List<int> occupiedRows = new List<int>();

			//verify placement i.t.o board edge and existing pieces
			for (int i = 0; i < toOccupy.GetLength(0); i++) {
				try {
					if (boardOccupation[toOccupy[i, 0], toOccupy[i, 1]]) {
						// Already occupied
						return false;
					}
					if (!occupiedRows.Contains(toOccupy[i, 1])) {
						occupiedRows.Add(toOccupy[i, 1]);
					}
				}
				catch (IndexOutOfRangeException) {
					// Not a valid placement
					return false;
				}
			}

			// Keep the game up to date
			game.ManuallyPlacePiece(piece, origin, orientation);
			occupiedRows.Sort();
			game.ClearRows(occupiedRows.ToArray(), new int[occupiedRows.Count]);

			for (int i = 0; i < toOccupy.GetLength(0); i++) {
				this.boardOccupation[toOccupy[i, 0], toOccupy[i, 1]] = true;
				boardFills[toOccupy[i, 0], toOccupy[i, 1]].Fill = piece.Brush;
			}

			//Clear the board as necessary, not optimized.
			List<int> rowsToOccupy = new List<int>();
			for (int i = 0; i < toOccupy.GetLength(0); i++) {
				if (!rowsToOccupy.Contains(toOccupy[i, 1])) {
					rowsToOccupy.Add(toOccupy[i, 1]);
				}
			}

			/*
	* The logic is pretty simple, we don't care about efficiency, so start at the bottom row, if it's full, remove it and bring down all the rows above.
	* Repeat for that row, once done, test the row up, until every row is tested.
	*/
			for (int i = this.height - 1; i >= 0; i--) {
				bool rowFull = true;
				for (int j = 0; j < this.width; j++) {
					if (!this.boardOccupation[j, i]) {
						rowFull = false;
						break;
					}
				}
				if (rowFull) {
					for (int j = i - 1; j >= 0; j--) { // Start at the row above to send it down 
						for (int x = 0; x < width; x++) { // Move horizontally along the board
							boardFills[x, j + 1].Fill = boardFills[x, j].Fill;
							//Grid.SetColumn(boardFills[x, j + 1], j + 1);

							this.boardOccupation[x, j + 1] = this.boardOccupation[x, j]; // set the board occupation for later verification.
						}
					}
					i++;
					for (int x = 0; x < width; x++) {
						boardFills[x, 0].Fill = transparentBrush;
						this.boardOccupation[x, 0] = false;
					}
				}
			}

			// Verify the two different calculations match
			if (!this.boardOccupation.ToString().Equals(this.game.Board.ToString())) {
				Console.WriteLine(this.boardOccupation.ToString());
				Console.WriteLine(this.game.Board.ToString());
				throw new Exception("TetrisGame and Tetris boards do not match");
			}

			display_score.Text = this.game.Score.ToString();

			return true;
		}

		private void Back_Click(object sender, RoutedEventArgs e) {
			Frame.GoBack();
		}

		private void newPiece() {
			switch (human_next.SelectedIndex) {
				case 0:
					currentPiece = new Piece();
					break;
				case 1:
					currentPiece = new Piece(Piece.PieceType.O);
					break;
				case 2:
					currentPiece = new Piece(Piece.PieceType.I);
					break;
				case 3:
					currentPiece = new Piece(Piece.PieceType.S);
					break;
				case 4:
					currentPiece = new Piece(Piece.PieceType.Z);
					break;
				case 5:
					currentPiece = new Piece(Piece.PieceType.L);
					break;
				case 6:
					currentPiece = new Piece(Piece.PieceType.J);
					break;
				case 7:
					currentPiece = new Piece(Piece.PieceType.T);
					break;
			}
			CurrentPieceText.Text = currentPiece.Letter.ToString();
		}

		private void Place_Click(object sender, RoutedEventArgs e) {
			if (!this.PlacePiece(currentPiece, new int[2] { Int32.Parse(human_x.Text), Int32.Parse(human_y.Text) }, human_orientation.SelectedIndex)) {
				return;
			}
			newPiece();
		}

		private void ClearBoard() {
			this.boardOccupation = new bool[width, height];
			for (int x = 0; x < this.width; x++) {
				for (int y = 0; y < this.height; y++) {
					boardFills[x, y].Fill = transparentBrush;
				}
			}
			this.game = new TetrisGame(TetrisSettings.Width, TetrisSettings.Height);
		}

		private void Restart(object sender, RoutedEventArgs e) {
			this.ClearBoard();
		}

		private void NNPlace(object sender, RoutedEventArgs e) {
			PlacementPackage[] packages = game.GetPackages(currentPiece);
			if (packages.Length == 0) {
				ClearBoard();
			}
			else {
				PlacementPackage bestPackage = packages[0];
				bestPackage.PackageScore = ann.Evaluate(bestPackage);
				for (int iPackage = 1; iPackage < packages.Length; iPackage++) {
					PlacementPackage package = packages[iPackage];
					package.PackageScore = ann.Evaluate(package);
					if (package.PackageScore > bestPackage.PackageScore) {
						bestPackage = package;
					}
				}

				int[,] toOccupy = bestPackage.OccupiedCells;
				this.package = bestPackage;

				List<int> occupiedRows = new List<int>();
				for (int i = 0; i < bestPackage.OccupiedCells.GetLength(0); i++) {
					if (!occupiedRows.Contains(toOccupy[i, 1])) {
						occupiedRows.Add(bestPackage.OccupiedCells[i, 1]);
					}
				}

				game.ApplyPackage(bestPackage);

				for (int i = 0; i < toOccupy.GetLength(0); i++) {
					if (this.boardOccupation[toOccupy[i, 0], toOccupy[i, 1]]) {
						//Console.WriteLine("thing");
						throw new Exception("Something about a thing");
					}
					this.boardOccupation[toOccupy[i, 0], toOccupy[i, 1]] = true;
					boardFills[toOccupy[i, 0], toOccupy[i, 1]].Fill = currentPiece.Brush;
				}

				//Clear the board as necessary, not optimized.
				List<int> rowsToOccupy = new List<int>();
				for (int i = 0; i < toOccupy.GetLength(0); i++) {
					if (!rowsToOccupy.Contains(toOccupy[i, 1])) {
						rowsToOccupy.Add(toOccupy[i, 1]);
					}
				}

				/*
					* The logic is pretty simple, we don't care about efficiency, so start at the bottom row, if it's full, remove it and bring down all the rows above.
					* Repeat for that row, once done, test the row up, until every row is tested.
					*/
				for (int i = this.height - 1; i >= 0; i--) {
					bool rowFull = true;
					for (int j = 0; j < this.width; j++) {
						if (!this.boardOccupation[j, i]) {
							rowFull = false;
							break;
						}
					}
					if (rowFull) {
						for (int j = i - 1; j >= 0; j--) { // Start at the row above to send it down 
							for (int x = 0; x < width; x++) { // Move horizontally along the board
								boardFills[x, j + 1].Fill = boardFills[x, j].Fill;
								//Grid.SetColumn(boardFills[x, j + 1], j + 1);

								this.boardOccupation[x, j + 1] = this.boardOccupation[x, j]; // set the board occupation for later verification.
							}
						}
						i++;
						for (int x = 0; x < width; x++) {
							boardFills[x, 0].Fill = transparentBrush;
							this.boardOccupation[x, 0] = false;
						}
					}
				}

				// Verify the two different calculations match
				if (!this.boardOccupation.ToString().Equals(this.game.Board.ToString())) {
					Console.WriteLine(this.boardOccupation.ToString());
					Console.WriteLine(this.game.Board.ToString());
					throw new Exception("TetrisGame and Tetris boards do not match");
				}
				display_score.Text = this.game.Score.ToString();
			}
			newPiece();
		}

		private void generateGrid(Int32 x, Int32 y) {
			this.width = x;
			this.height = y;

			for (int i = 0; i < y; i++) {
				board.RowDefinitions.Add(new RowDefinition());
			}

			for (int i = 0; i < x; i++) {
				board.ColumnDefinitions.Add(new ColumnDefinition());
			}
		}
	}

	public class TetrisInput {
		public Int32 height { get; set; } = 20;
		public Int32 width { get; set; } = 10;

		public bool human { get; set; } = false;
		public Int32 iterations { get; set; } = 1;
	}
}
