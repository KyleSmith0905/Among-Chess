namespace AmongChess.Source.Chess.Pieces
{
	internal class Knight
	{
		public static bool CanMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			int[] moveX = new int[] { -2, -1, -2, -1, 2, 1, 2, 1 };
			int[] moveY = { -1, -2, 1, 2, -1, -2, 1, 2 };
			for (int index = 0; index < 8; index++)
			{
				(int x, int y) coordinate = (x: moveX[index] + fromCoordinates.x, y: moveY[index] + fromCoordinates.y);
				if (coordinate.x == toCoordinates.x && coordinate.y == toCoordinates.y) return true;
			}
			return false;
		}

		public static bool CanEscape(char[,] chessBoard, (int x, int y) coordinates)
		{
			int[] moveX = new int[] { -2, -1, -2, -1, 2, 1, 2, 1 };
			int[] moveY = new int[] { -1, -2, 1, 2, -1, -2, 1, 2 };
			for (int i = 0; i < 8; i++)
			{
				(int x, int y) testCoordinate = (moveX[i] + coordinates.x, moveY[i] + coordinates.y);
				if (Validation.CheckBounds(testCoordinate.x, testCoordinate.y)) continue;
				char attackPiece = chessBoard[testCoordinate.y, testCoordinate.x];
				if (attackPiece == '1' || char.IsUpper(attackPiece) != char.IsUpper(chessBoard[coordinates.y, coordinates.x])) return true;
			}
			return false;
		}
	}
}
