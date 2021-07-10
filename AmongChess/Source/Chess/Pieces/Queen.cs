using System;
using UnityEngine;

namespace AmongChess.Source.Chess.Pieces
{
	internal class Queen
	{
		public static bool CanMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			return Rook.CanMove(fromCoordinates, toCoordinates) || Bishop.CanMove(fromCoordinates, toCoordinates);
		}

		public static bool CanEscape(char[,] chessBoard, (int x, int y) coordinates)
		{
			for (int i = 0; i < 8; i++)
			{
				(int x, int y) testCoordinate = (Mathf.RoundToInt((float)Math.Sin(i * Math.PI * 0.25)) + coordinates.x, Mathf.RoundToInt((float)Math.Cos(i * Math.PI * 0.25)) + coordinates.y);
				if (Validation.CheckBounds(testCoordinate.x, testCoordinate.y)) continue;
				char attackPiece = chessBoard[testCoordinate.y, testCoordinate.x];
				if (char.IsUpper(attackPiece) != char.IsUpper(chessBoard[coordinates.y, coordinates.x]) || attackPiece == '1') return true;
			}
			return false;
		}
	}
}
