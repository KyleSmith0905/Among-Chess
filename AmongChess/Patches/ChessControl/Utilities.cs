using System;

namespace AmongChess.Patches.ChessControl
{
	internal class Utilities
	{
		public static char[] FromTranslate = new char[] { 'f', 'F', 'e', 'E', 'h', 'H', 'm', 'M' };
		public static char[] ToTranslate = new char[] { 'p', 'P', 'p', 'P', 'r', 'R', 'k', 'K' };

		public static char ReadablePiece(char character)
		{
			int index = Array.IndexOf(FromTranslate, character);
			return index == -1 ? character : ToTranslate[index];
		}

		public static char[,] ReadableBoard(char[,] chessBoard)
		{
			char[,] tempChessBoard = (char[,])chessBoard.Clone();
			for (int y = 0; y < tempChessBoard.GetLength(0); y++)
			{
				for (int x = 0; x < tempChessBoard.GetLength(1); x++)
				{
					tempChessBoard[y, x] = ReadablePiece(tempChessBoard[y, x]);
				}
			}
			return tempChessBoard;
		}

		public static (int x, int y) KingFinder(bool isWhite, char[,] tempChessBoard)
		{
			for (int y = 0; y < tempChessBoard.GetLength(0); y++)
			{
				for (int x = 0; x < tempChessBoard.GetLength(1); x++)
				{
					char piece = tempChessBoard[y, x];
					if (piece == '1' || (isWhite && !char.IsUpper(piece)) || (!isWhite && char.IsUpper(piece))) continue;
					else if (char.ToUpper(piece) == 'K' || char.ToUpper(piece) == 'M') return (x, y);
				}
			}
			return (x: -1, y: -1);
		}

		public static EnumMoves GetHowMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			EnumMoves howMove = EnumMoves.Normal;
			int directionY = char.IsUpper(Control.ChessBoard[fromCoordinates.y, fromCoordinates.x]) ? -1 : 1;
			float halfRank = Control.ChessBoard.GetLength(0) * 0.5f;
			if (char.ToUpper(Control.ChessBoard[fromCoordinates.y, fromCoordinates.x]) == 'M' && char.ToUpper(Control.ChessBoard[toCoordinates.y, toCoordinates.x]) == 'H' && fromCoordinates.y == toCoordinates.y) howMove = Math.Sign(toCoordinates.x - fromCoordinates.x) == 1 ? EnumMoves.KingCastle : EnumMoves.QueenCastle;
			else if (char.ToUpper(ReadablePiece(Control.ChessBoard[fromCoordinates.y, fromCoordinates.x])) == 'P' && char.ToUpper(Control.ChessBoard[fromCoordinates.y, toCoordinates.x]) == 'E') howMove = EnumMoves.EnPassant;
			else if (char.ToUpper(ReadablePiece(Control.ChessBoard[fromCoordinates.y, fromCoordinates.x])) == 'P' && toCoordinates.y == halfRank + (halfRank * directionY)) howMove = EnumMoves.Promotion;
			return howMove;
		}
	}
}
