using System;
using UnityEngine;

namespace AmongChess.Source.Chess.Pieces
{
	internal class Bishop
	{
		public static bool CanMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			for (int index = 0; index < 4; index++)
			{
				int directionY = Mathf.RoundToInt((float)Math.Sin((index * Math.PI * 0.5) + (Math.PI * 0.25)));
				int directionX = Mathf.RoundToInt((float)Math.Cos((index * Math.PI * 0.5) + (Math.PI * 0.25)));
				for (int i = 1; i < 8; i++)
				{
					(int x, int y) coordinate = ((directionX * i) + fromCoordinates.x, (directionY * i) + fromCoordinates.y);
					if (Validation.CheckBounds(coordinate)) break;
					else if (coordinate.x == toCoordinates.x && coordinate.y == toCoordinates.y) return true;
					else if (Chess.ChessBoard[coordinate.y, coordinate.x] == '1') continue;
					else break;
				}
			}
			return false;
		}

		public static bool CanEscape(char[,] chessBoard, (int x, int y) coordinates)
		{
			for (int i = 0; i < 4; i++)
			{
				(int x, int y) testCoordinate = (Mathf.RoundToInt((float)Math.Sin((i * Math.PI * 0.5) + (Math.PI * 0.25))) + coordinates.x, Mathf.RoundToInt((float)Math.Cos((i * Math.PI * 0.5) + (Math.PI * 0.25))) + coordinates.y);
				if (Validation.CheckBounds(testCoordinate.x, testCoordinate.y)) continue;
				char attackPiece = chessBoard[testCoordinate.y, testCoordinate.x];
				if (char.IsUpper(attackPiece) != char.IsUpper(chessBoard[coordinates.y, coordinates.x]) || attackPiece == '1') return true;
			}
			return false;
		}
	}
}
