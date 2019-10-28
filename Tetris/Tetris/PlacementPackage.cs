using System;

namespace Tetris {
	public class PlacementPackage {
		public bool[][] ResultantBoard { get; }
		public double RemovedRows { get; set; }
		public double Smoothness { get; set; }
		public double LandingHeight { get; set; }
		public double NumberOfHoles { get; set; }
		public double NumberOfWells { get; set; }
		public double AltitudeDifference { get; set; }
		public double PileHeight { get; set; }
		public double MaxWellDepth { get; set; }
		public double TotalWellDepth { get; set; }
		public double Covered { get; set; }
		public double WeightedBlockCount { get; set; }
		public double ConnectedHoleCount { get; set; }
		public double BlockCount { get; set; }
		public double ErodedPieceCount { get; set; }
		public double RowTransitions { get; set; }
		public double ColumnTransitions { get; set; }

		private bool normal = false;
		private int score = 0;

		public int[] Peaks { get; set; }

		public int[,] OccupiedCells { get; set; }

		public double PackageScore { get; set; }


		public PlacementPackage(bool[][] ResultantBoard) {
			RemovedRows = 0;
			Smoothness = 0;
			LandingHeight = 0;
			NumberOfHoles = 0;
			NumberOfWells = 0;
			PackageScore = 0;
			AltitudeDifference = 0;
			PileHeight = 0;
			TotalWellDepth = 0;
			MaxWellDepth = 0;
			Covered = 0;
			WeightedBlockCount = 0;
			ConnectedHoleCount = 0;
			BlockCount = 0;
			ErodedPieceCount = 0;
			RowTransitions = 0;
			ColumnTransitions = 0;

			normal = false;

			this.ResultantBoard = ResultantBoard;
		}

		public PlacementPackage() {
			RemovedRows = 0;
			Smoothness = 0;
			LandingHeight = 0;
			NumberOfHoles = 0;
			NumberOfWells = 0;
			PackageScore = 0;
			AltitudeDifference = 0;
			PileHeight = 0;
			TotalWellDepth = 0;
			MaxWellDepth = 0;
			Covered = 0;
			WeightedBlockCount = 0;
			ConnectedHoleCount = 0;
			BlockCount = 0;
			ErodedPieceCount = 0;
			RowTransitions = 0;
			ColumnTransitions = 0;

			normal = false;
		}

		public void Normalize() {
			if (!normal) {
				score = (int)RemovedRows;
				double min = RemovedRows;
				double max = RemovedRows;
				for (int i = 1; i < ANNSettings.Input; i++) {
					double feature = GetFeature(i);
					min = feature < min ? feature : min;
					max = feature > max ? feature : max;
				}

				double bot = max - min;
				Smoothness = (Smoothness - min) / bot;
				LandingHeight = (LandingHeight - min) / bot;
				NumberOfHoles = (NumberOfHoles - min) / bot;
				NumberOfWells = (NumberOfWells - min) / bot;
				RemovedRows = (RemovedRows - min) / bot;
				AltitudeDifference = (AltitudeDifference - min) / bot;
				PileHeight = (PileHeight - min) / bot;
				MaxWellDepth = (MaxWellDepth - min) / bot;
				TotalWellDepth = (TotalWellDepth - min) / bot;
				Covered = (Covered - min) / bot;
				WeightedBlockCount = (WeightedBlockCount - min) / bot;
				ConnectedHoleCount = (ConnectedHoleCount - min) / bot;
				BlockCount = (BlockCount - min) / bot;
				ErodedPieceCount = (ErodedPieceCount - min) / bot;
				RowTransitions = (RowTransitions - min) / bot;
				ColumnTransitions = (ColumnTransitions - min) / bot;
				normal = true;
			}
		}

		public int GetScore() {
			return score;
		}

		public double GetFeature(int at) {
			switch (at) {
				case 0:
					return RemovedRows;
				case 1:
					return Smoothness;
				case 2:
					return LandingHeight;
				case 3:
					return NumberOfHoles;
				case 4:
					return NumberOfWells;
				case 5:
					return AltitudeDifference;
				case 6:
					return PileHeight;
				case 7:
					return MaxWellDepth;
				case 8:
					return TotalWellDepth;
				case 9:
					return Covered;
				case 10:
					return WeightedBlockCount;
				case 11:
					return ConnectedHoleCount;
				case 12:
					return BlockCount;
				case 13:
					return ErodedPieceCount;
				case 14:
					return RowTransitions;
				case 15:
					return ColumnTransitions;
				default:
					return -1; // bias unit
			}
		}
	}
}
