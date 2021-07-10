namespace AmongChess.Source.Chess.Pieces
{
	internal class Pawn
	{
		public static bool CanMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates, ref EnumMoves howMove)
		{
			char[,] chessBoard = Chess.ChessBoard;
			int directionY = char.IsUpper(chessBoard[fromCoordinates.y, fromCoordinates.x]) ? -1 : 1;
			char pawnType = chessBoard[fromCoordinates.y, fromCoordinates.x];
			float halfRank = (chessBoard.GetLength(0) * 0.5f) - 0.5f; ;
			if (toCoordinates.y == halfRank + (halfRank * directionY))
			{
				howMove = EnumMoves.Promotion;
			}
			if (fromCoordinates.x == toCoordinates.x && fromCoordinates.y + directionY == toCoordinates.y)
			{
				return chessBoard[toCoordinates.y, toCoordinates.x] == '1';
			}
			else if (fromCoordinates.x == toCoordinates.x && fromCoordinates.y + (directionY * 2) == toCoordinates.y && char.ToUpper(pawnType) == 'F')
			{
				return chessBoard[fromCoordinates.y + directionY, toCoordinates.x] == '1' && chessBoard[toCoordinates.y, toCoordinates.x] == '1';
			}
			for (int directionX = -1; directionX < 2; directionX += 2)
			{
				(int x, int y) testCoordinates = (fromCoordinates.x + directionX, fromCoordinates.y + directionY);
				if (testCoordinates.x == toCoordinates.x && testCoordinates.y == toCoordinates.y && !Validation.CheckBounds(testCoordinates) && chessBoard[testCoordinates.y, testCoordinates.x] != '1') return true;
			}
			for (int directionX = -1; directionX < 2; directionX += 2)
			{
				(int x, int y) testCoordinates = (fromCoordinates.x + directionX, fromCoordinates.y + directionY);
				if (testCoordinates.x == toCoordinates.x && testCoordinates.y == toCoordinates.y && chessBoard[toCoordinates.y, toCoordinates.x] == '1' && char.ToUpper(chessBoard[fromCoordinates.y, toCoordinates.x]) == 'E')
				{
					howMove = EnumMoves.EnPassant;
					return true;
				}
			}
			return false;
		}

		public static bool CanEscape(char[,] chessBoard, (int x, int y) coordinates)
		{
			int directionY = char.IsUpper(chessBoard[coordinates.y, coordinates.x]) ? -1 : 1;
			if (!Validation.CheckBounds(coordinates.x, directionY + coordinates.y) && chessBoard[directionY + coordinates.y, coordinates.x] == '1')
			{
				return true;
			}
			else if (!Validation.CheckBounds(coordinates.x, (directionY * 2) + coordinates.y) && char.ToUpper(chessBoard[coordinates.y, coordinates.x]) == 'F' && char.ToUpper(chessBoard[(directionY * 2) + coordinates.y, coordinates.x]) == '1')
			{
				return true;
			}
			else
			{
				for (int i = -1; i < 2; i += 2)
				{
					if ((!Validation.CheckBounds(coordinates.x + i, directionY + coordinates.y) && chessBoard[directionY + coordinates.y, coordinates.x + i] != '1' && char.IsUpper(chessBoard[directionY + coordinates.y, coordinates.x + i]) != char.IsUpper(chessBoard[coordinates.y, coordinates.x])) || (!Validation.CheckBounds(coordinates.x + i, coordinates.y) && chessBoard[coordinates.y, coordinates.x + i] == 'E')) return true;
				}
			}
			return false;
		}
	}
}
