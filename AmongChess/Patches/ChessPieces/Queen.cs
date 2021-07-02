namespace AmongChess.Patches.ChessPieces
{
	internal class Queen
	{
		public static bool CanMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			return Rook.CanMove(fromCoordinates, toCoordinates) || Bishop.CanMove(fromCoordinates, toCoordinates);
		}

		public static bool CanEscape(char[,] chessBoard, (int x, int y) coordinates)
		{
			return King.CanEscape(chessBoard, coordinates);
		}
	}
}
