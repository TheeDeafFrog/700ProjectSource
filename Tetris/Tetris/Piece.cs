using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Tetris {

	public class Piece {
		public enum PieceType {
			O, I, S, Z, L, J, T
		};

		private readonly PieceType pieceType;
		private readonly static SolidColorBrush
			yellowBrush = new SolidColorBrush(Colors.Yellow),
			cyanBrush = new SolidColorBrush(Colors.Cyan),
			greenBrush = new SolidColorBrush(Colors.Green),
			redBrush = new SolidColorBrush(Colors.Red),
			orangeBrush = new SolidColorBrush(Colors.Orange),
			blueBrush = new SolidColorBrush(Colors.Blue),
			purpleBrush = new SolidColorBrush(Colors.Purple);

		public Piece() {
			Array values = Enum.GetValues(typeof(PieceType));
			this.pieceType = (PieceType)values.GetValue(Rand.Next(0, values.Length));
		}

		public Piece(PieceType piece) {
			this.pieceType = piece;
		}

		public char Letter {
			get {
				switch (this.pieceType) {
					case PieceType.O:
						return 'O';
					case PieceType.I:
						return 'I';
					case PieceType.S:
						return 'S';
					case PieceType.Z:
						return 'Z';
					case PieceType.L:
						return 'L';
					case PieceType.J:
						return 'J';
					case PieceType.T:
						return 'T';
				}
				return ' ';
			}
		}

		public Brush Brush {
			get {
				switch (this.pieceType) {
					case PieceType.O:
						return yellowBrush;
					case PieceType.I:
						return cyanBrush;
					case PieceType.S:
						return greenBrush;
					case PieceType.Z:
						return redBrush;
					case PieceType.L:
						return orangeBrush;
					case PieceType.J:
						return blueBrush;
					default:
						return purpleBrush;
				}
			}
		}

		public int FinalOrientation {
			get {
				switch (this.pieceType) {
					case PieceType.O:
						return 0;
					case PieceType.I:
					case PieceType.S:
					case PieceType.Z:
						return 1;
					case PieceType.L:
					case PieceType.J:
					case PieceType.T:
					default:
						return 3;
				}
			}
		}

		/**
		 * Given a pair of ints (x, y) and an orientation
		 * Return an array of pairs, where each pair is a cell that will be occupied as a result of the piece being placed on that cell.
		 * */
		public int[,] DetermineCellsToOccupy(int[] originHere, int orientation) {
			switch (this.pieceType) {
				case PieceType.O:
					return new int[4, 2] {
						{originHere[0] - 1, originHere[1] },
						{originHere[0], originHere[1] },
						{originHere[0] - 1, originHere[1] + 1 },
						{originHere[0], originHere[1] + 1 }
					};
				case PieceType.I:
					if (orientation == 0) {
						return new int[4, 2] {
							{ originHere[0] - 2, originHere[1] },
							{ originHere[0] - 1, originHere[1] },
							{ originHere[0], originHere[1] },
							{ originHere[0] + 1, originHere[1] }
						};
					}
					else {
						return new int[4, 2] {
							{ originHere[0], originHere[1] - 1 },
							{ originHere[0], originHere[1] },
							{ originHere[0], originHere[1] + 1 },
							{ originHere[0], originHere[1] + 2 }
						};
					}
				case PieceType.S:
					if (orientation == 0) {
						return new int[4, 2] {
							{ originHere[0] + 1, originHere[1] },
							{ originHere[0], originHere[1] },
							{ originHere[0], originHere[1] + 1 },
							{ originHere[0] - 1, originHere[1] + 1 }
						};
					}
					else {
						return new int[4, 2] {
							{ originHere[0], originHere[1] - 1 },
							{ originHere[0], originHere[1] },
							{ originHere[0] + 1, originHere[1] },
							{ originHere[0] + 1, originHere[1] + 1}
						};
					}
				case PieceType.Z:
					if (orientation == 0) {
						return new int[4, 2] {
							{ originHere[0] - 1, originHere[1] },
							{ originHere[0], originHere[1] },
							{ originHere[0], originHere[1] + 1},
							{ originHere[0] + 1, originHere[1] + 1}
						};
					}
					else {
						return new int[4, 2] {
							{ originHere[0] + 1, originHere[1] - 1},
							{ originHere[0] + 1, originHere[1] },
							{ originHere[0], originHere[1] },
							{ originHere[0], originHere[1] + 1}
						};
					}
				case PieceType.L:
					switch (orientation) {
						case 0:
							return new int[4, 2] {
								{ originHere[0] - 1, originHere[1] + 1},
								{ originHere[0] - 1, originHere[1] },
								{ originHere[0], originHere[1] },
								{ originHere[0] + 1, originHere[1] }
							};
						case 2:
							return new int[4, 2] {
								{ originHere[0] + 1, originHere[1] - 1},
								{ originHere[0], originHere[1] },
								{ originHere[0] + 1, originHere[1] },
								{ originHere[0] - 1, originHere[1] }
							};
						case 1:
							return new int[4, 2] {
								{ originHere[0] + 1, originHere[1] + 1},
								{ originHere[0], originHere[1] },
								{ originHere[0], originHere[1] + 1 },
								{ originHere[0], originHere[1] - 1}
							};
						default:
							return new int[4, 2] {
								{ originHere[0] - 1, originHere[1] - 1},
								{ originHere[0], originHere[1] },
								{ originHere[0], originHere[1] + 1},
								{ originHere[0], originHere[1] - 1}
							};
					}
				case PieceType.J:
					switch (orientation) {
						case 0:
							return new int[4, 2] {
								{ originHere[0] + 1, originHere[1] + 1},
								{ originHere[0] + 1, originHere[1] },
								{ originHere[0] - 1, originHere[1] },
								{ originHere[0], originHere[1] }
							};
						case 1:
							return new int[4, 2] {
								{ originHere[0] + 1, originHere[1] - 1},
								{ originHere[0], originHere[1] - 1},
								{ originHere[0], originHere[1] + 1},
								{ originHere[0], originHere[1] }
							};
						case 2:
							return new int[4, 2] {
								{ originHere[0] - 1, originHere[1] - 1},
								{ originHere[0] - 1, originHere[1] },
								{ originHere[0] + 1, originHere[1] },
								{ originHere[0], originHere[1] }
							};
						default:
							return new int[4, 2] {
								{ originHere[0] - 1, originHere[1]  + 1},
								{ originHere[0], originHere[1]  + 1},
								{ originHere[0], originHere[1] - 1},
								{ originHere[0], originHere[1] }
							};
					}
				default:
					switch (orientation) {
						case 0:
							return new int[4, 2] {
								{ originHere[0] + 1, originHere[1] },
								{ originHere[0] - 1, originHere[1] },
								{ originHere[0], originHere[1] + 1},
								{ originHere[0], originHere[1] }
							};
						case 1:
							return new int[4, 2] {
								{ originHere[0] + 1, originHere[1] },
								{ originHere[0], originHere[1] + 1},
								{ originHere[0], originHere[1] - 1},
								{ originHere[0], originHere[1] }
							};
						case 2:
							return new int[4, 2] {
								{ originHere[0] + 1, originHere[1] },
								{ originHere[0] - 1, originHere[1] },
								{ originHere[0], originHere[1] - 1},
								{ originHere[0], originHere[1] }
							};
						default:
							return new int[4, 2] {
								{ originHere[0] - 1, originHere[1] },
								{ originHere[0], originHere[1] + 1},
								{ originHere[0], originHere[1] - 1},
								{ originHere[0], originHere[1] }
							};
					}
			}
		}

		internal List<int[,]> GetPlacements(int x, Func<object, int> p) {
			throw new NotImplementedException();
		}

		public int DetermineLowestPoint(int y, int orientation) {
			switch (this.pieceType) {
				case PieceType.S:
				case PieceType.Z:
				case PieceType.O:
					return y - 1;
				case PieceType.I:
					switch (orientation) {
						case 0:
							return y;
						default:
							return y - 2;
					}
				case PieceType.J:
				case PieceType.T:
				default:
					switch (orientation) {
						case 2:
							return y;
						default:
							return y - 1;
					}
			}
		}

		/*
		 * 3D:
		 *		x,?,? represents a placement of the piece
		 *		?,x,? is a cell
		 *		?,?,0 is the x
		 *		?,?,1 is the y
		 *		
		 *	Returns all possible combinations of cells that can be occupied by the piece if the given cell is the lowest point that can be occupied by the piece, and consequently, is currently unoccupied.
		 */
		public List<int[,]> GetPlacements(int x, int y) {
			List<int[,]> result = new List<int[,]>();
			switch (this.pieceType) {
				case PieceType.O:
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y - 1 }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 0));
					return result;
				case PieceType.I:
					result.Add(DetermineCellsToOccupy(new int[] { x + 2, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 2 }, 1));
					return result;
				case PieceType.S:
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y - 1 }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y }, 1));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y - 1 }, 1));
					return result;
				case PieceType.Z:
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y - 1 }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 1));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y }, 1));
					return result;
				case PieceType.L:
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y - 1 }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 1));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y - 1 }, 1));
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y }, 2));
					result.Add(DetermineCellsToOccupy(new int[] { x, y }, 2));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y }, 2));
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y + 1 }, 3));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 3));
					return result;
				case PieceType.J:
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y - 1 }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 1));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y + 1 }, 1));
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y }, 2));
					result.Add(DetermineCellsToOccupy(new int[] { x, y }, 2));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y }, 2));
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y - 1 }, 3));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 3));
					return result;
				default:
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y }, 0));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 1));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y }, 1));
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y }, 2));
					result.Add(DetermineCellsToOccupy(new int[] { x, y }, 2));
					result.Add(DetermineCellsToOccupy(new int[] { x - 1, y }, 2));
					result.Add(DetermineCellsToOccupy(new int[] { x + 1, y }, 3));
					result.Add(DetermineCellsToOccupy(new int[] { x, y - 1 }, 3));
					return result;
			}
		}

	}
}
