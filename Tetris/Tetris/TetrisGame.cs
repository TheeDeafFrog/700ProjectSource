using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetris {
	public class TetrisGame {

		private bool[][] board;
		public bool[,] Board {
			get {
				bool[,] result = new bool[Width, Height];
				for (int x = 0; x < Width; x++) {
					for (int y = 0; y < Height; y++) {
						result[x, y] = board[y][x];
					}
				}
				return result;
			}
			set {
				for (int x = 0; x < Width; x++) {
					for (int y = 0; y < Height; y++) {
						board[y][x] = value[x, y];
					}
				}
			}
		}

		private int Height, Width;
		public double Score { get; set; } = 0; // The number of lines cleared, no schenanigans
		public double PlacedPieces { get; set; } = 0;
		private int[] peaks;

		public void ResetGame(int width, int height) {
			this.Width = width;
			this.Height = height;

			this.peaks = Enumerable.Repeat(Height - 1, Width).ToArray(); // the space above the top occupied cell in the column.
			this.board = new bool[Height][];
			for (int y = 0; y < Height; y++) {
				this.board[y] = new bool[Width];
			}

			this.Score = 0;
			this.PlacedPieces = 0;
		}

		public TetrisGame(int width, int height) {
			this.Height = height;
			this.Width = width;
			this.peaks = Enumerable.Repeat(height - 1, width).ToArray(); // the space above the top occupied cell in the column.

			this.board = new bool[height][];
			for (int y = 0; y < Height; y++) {
				this.board[y] = new bool[width];
			}
		}

		#region Row Removal
		public Tuple<int, int> ClearRows(int[] rowsWithPlacements, int[] cells, bool[][] board = null) {
			board = board ?? this.board;
			int rowsCleared = 0;
			int cellsEroded = 0;
			Array.Sort(rowsWithPlacements);
			//for (int p = 0; p < rowsWithPlacements.Length; p++) {
			//	int row = rowsWithPlacements[p];
			//	if (board[row].All(r => r)) {
			//		rowsCleared++;
			//		cellsEroded += cells[p];
			//		for (int r = row; r > 0; r--) {
			//			board[r] = board[r - 1];
			//		}
			//		board[0] = new bool[Width];
			//		for (int j = p + 1; j < rowsWithPlacements.Length; j++) {
			//			rowsWithPlacements[j] += 1;
			//		}

			//	}
			//}
			for (int r = 0; r < rowsWithPlacements.Length; r++) {
				int row = rowsWithPlacements[r];
				if (board[row].All(o => o)) {
					rowsCleared++;
					cellsEroded += cells[r];
					for (int s = row; s > 0; s--) {
						board[s] = board[s - 1];
					}
					board[0] = new bool[Width];
				}
			}
			return new Tuple<int, int>(rowsCleared, cellsEroded);
		}
		#endregion

		#region Manual Control
		/**
		 * Assumes valid placement
		 */
		public void ManuallyPlacePiece(Piece piece, int[] origin, int orientation) {
			int[,] placements = piece.DetermineCellsToOccupy(origin, orientation);
			for (int iCell = 0; iCell < 4; iCell++) {
				this.board[placements[iCell, 1]][placements[iCell, 0]] = true;
			}
		}
		#endregion

		#region Package
		private int LowestPoint(int[,] placement) {
			int lowest = 0;
			for (int i = 0; i < placement.GetLength(0); i++) {
				if (placement[i, 1] > lowest) {
					lowest = placement[i, 1];
				}
			}
			return lowest;
		}
		private static bool[][] CopyArray(bool[][] source) {
			int len = source.Length;
			bool[][] dest = new bool[len][];
			for (int i = 0; i < len; i++) {
				bool[] inner = source[i];
				int iLen = inner.Length;
				bool[] newer = new bool[iLen];
				Array.Copy(inner, newer, iLen);
				dest[i] = newer;
			}

			return dest;
		}
		public PlacementPackage[] GetPackages(Piece piece = null) {
			/* Features:
			 * Removed Rows - Number of rows cleared as result of placement
			 * Smoothness - Sum of difference in height between adjacent columns
			 * Landing Height - The height where the piece is being placed
			 * Number of Holes - Number of surrounded cells (other pieces or edge)
			 * Number of Wells - Cell is closed at bottom, left, and right; but open on the top
			 * Altitude Difference - Highest peak minus lowest peak
			 * Pile Height - Row of highest occupied cell
			 * Maximum Well Depth - The depth of the deepest well
			 * Sum of Well Depths - The sum of all well depths
			 * Covered - The number of empty cells below an occupied cell
			 * Weighted Block Count - The sum of all occupied cells, weighted by the row in which it occurs
			 * Connected Hole Count - Several vertically connected gaps make one connected hole
			 * Block Count - The total number of blocks in the grid.
			 * Eroded Piece Count - cells from current placement removed, multiplied by removed rows.
			 * Row Transitions - The number of transitions between occupied and unoccupied cells in a row, edges are occupied.
			 * Column Transitions - Same as row, but for column.
			 */

			List<PlacementPackage> packages = new List<PlacementPackage>();
			piece = piece ?? new Piece();
			for (int testX = 0; testX < Width; testX++) {
				List<int[,]> placements = new List<int[,]>();
				for (int y = this.peaks[testX]; y < Height; y++) {
					if (y >= 0) {
						if (!board[y][testX]) {
							placements.AddRange(piece.GetPlacements(testX, y));
						}
					}
				}

				foreach (int[,] placement in placements) {
					bool valid = false;
					List<int> rowsWithPlacements = new List<int>();
					List<int> cells = new List<int>();
					for (int iCell = 0; iCell < placement.GetLength(0); iCell++) {
						int y = placement[iCell, 1], x = placement[iCell, 0];
						if (x < 0 || x >= Width || y < 0 || y >= Height) {
							valid = false;
							break;
						}
						if (board[y][x]) {
							valid = false;
							break;
						}

						if (y == Height - 1) {
							valid = true;
						}
						else if (board[y + 1][x]) {
							valid = true;
						}

						if (!rowsWithPlacements.Contains(y)) {
							rowsWithPlacements.Add(y);
							cells.Add(1);
						}
						else {
							cells[rowsWithPlacements.IndexOf(y)]++;
						}
					}
					if (valid) {
						bool[][] result = CopyArray(this.board);
						for (int iCell = 0; iCell < placement.GetLength(0); iCell++) {
							result[placement[iCell, 1]][placement[iCell, 0]] = true;
						}
						Tuple<int, int> clearResult = ClearRows(rowsWithPlacements.ToArray(), cells.ToArray(), result);
						PlacementPackage package = new PlacementPackage(result) {
							// Removed Rows
							RemovedRows = clearResult.Item1,
							// Eroded Piece Count
							ErodedPieceCount = clearResult.Item1 * clearResult.Item2,
							OccupiedCells = placement
						};
						int[] peaks = Enumerable.Repeat(Height - 1, Width).ToArray();
						int lowPeak = 0;
						int highPeak = TetrisSettings.Height;
						bool rPrev = true, cPrev = true;
						for (int y = 0; y < Height; y++) {
							for (int x = 0; x < Width; x++) {
								if (this.board[y][x] != rPrev) {
									rPrev = !rPrev;
									// Row Transitions
									package.RowTransitions++;
								}
							}
							if (this.board[y][Width - 1] == false) {
								// Row Transitions
								package.RowTransitions++;
							}
						}
						for (int x = 0; x < Width; x++) {
							for (int y = 0; y < Height; y++) {
								if (this.board[y][x] != cPrev) {
									// Column Transitions
									package.ColumnTransitions++;
									cPrev = !cPrev;
								}
								if (peaks[x] == Height - 1 && result[y][x]) {
									peaks[x] = y - 1;
									highPeak = peaks[x] < highPeak ? peaks[x] : highPeak;
									lowPeak = peaks[x] > lowPeak ? peaks[x] : lowPeak;
									int depth = 0;
									for (int i = y - 1; i >= 0; i--) {
										bool[] res = result[i];
										int l = x - 1, r = x + 1;
										if ((l < 0 || res[l]) && (r == Width || res[r])) {
											depth++;
										}
										else {
											break;
										}
									}
									if (depth > 0) {
										// Number of Wells
										package.NumberOfWells++;
										// Sum of all Well depths
										package.TotalWellDepth += depth;
										
										// Max Well Depth
										if (depth > package.MaxWellDepth) {
											package.MaxWellDepth = depth;
										}
									}
								}
								if (peaks[x] != Height - 1) {
									if (!result[y][x]) {
										int r = x + 1, l = x - 1, u = y - 1, d = y + 1;
										bool[] res = result[y];
										// Covered
										package.Covered++;
										if ((r == Width || res[r]) && (l < 0 || res[l]) && (u < 0 || result[u][x])) {
											if (d == Height || result[d][x]) {
												// Number of Holes
												package.NumberOfHoles++;
											}
											else {
												// Connected Hole Count
												package.ConnectedHoleCount++;
											}
										}
									}
									else {
										// Weighted Block Count
										package.WeightedBlockCount += y;
										// Block Count
										package.BlockCount++;
									}
								}
								else {
									lowPeak = Height - 1;
								}
							}
							if (this.board[Height - 1][x] == false) {
								package.ColumnTransitions++;
							}
						}

						for (int x = 0; x < Width - 1; x++) {
							// Smoothness
							package.Smoothness += Math.Abs(peaks[x] - peaks[x + 1]);
						}

						// Landing Height
						package.LandingHeight = Height - LowestPoint(placement);

						//Altitude Difference
						package.AltitudeDifference = Math.Abs(highPeak - lowPeak);

						// Pile Height
						package.PileHeight = TetrisSettings.Height - highPeak;

						package.Peaks = peaks;
						packages.Add(package);
					}
				}
			}
			return packages.ToArray();
		}
		public void ApplyPackage(PlacementPackage package) {
			this.board = package.ResultantBoard;
			this.peaks = package.Peaks;
			this.Score += package.GetScore();
			this.PlacedPieces++;
		}
		#endregion

	}
}
