using System;
using UnityEngine;

namespace AmongChess.Source.Chess.Pieces
{
	internal class King
	{
		public static bool CanMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates, ref EnumMoves howMove)
		{
			char[,] chessBoard = Chess.ChessBoard;
			for (int index = 0; index < 8; index++)
			{
				if (char.ToUpper(chessBoard[toCoordinates.y, toCoordinates.x]) == 'H') break;
				int directionY = Mathf.RoundToInt((float)Math.Sin(index * Math.PI * 0.25));
				int directionX = Mathf.RoundToInt((float)Math.Cos(index * Math.PI * 0.25));
				(int x, int y) testCoordinates = (x: directionX + fromCoordinates.x, y: directionY + fromCoordinates.y);
				if (Validation.CheckBounds(testCoordinates)) continue;
				else if (testCoordinates.x == toCoordinates.x && testCoordinates.y == toCoordinates.y) return true;
			}
			if (char.ToUpper(chessBoard[fromCoordinates.y, fromCoordinates.x]) == 'M' && char.ToUpper(chessBoard[toCoordinates.y, toCoordinates.x]) == 'H' && fromCoordinates.y == toCoordinates.y)
			{
				int directionX = Math.Sign(toCoordinates.x - fromCoordinates.x);
				int endPosition = directionX == 1 ? 6 : 2;
				int numOfRooks = 0;
				bool ifOne = chessBoard[fromCoordinates.y, endPosition] != '1' && endPosition != fromCoordinates.x && endPosition != toCoordinates.x;
				bool ifTwo = chessBoard[fromCoordinates.y, endPosition - directionX] != '1' && endPosition - directionX != fromCoordinates.x && endPosition - directionX != toCoordinates.x;
				if (ifOne || ifTwo) return false;
				for (int i = 0; i < 8; i++)
				{
					(int x, int y) testCoordinates = (fromCoordinates.x + (directionX * i), fromCoordinates.y);
					if (Validation.CheckBounds(testCoordinates)) return false;
					char[,] tempChessBoard = (char[,])chessBoard.Clone();
					tempChessBoard[fromCoordinates.y, fromCoordinates.x] = '1';
					tempChessBoard[testCoordinates.y, testCoordinates.x] = char.IsUpper(chessBoard[fromCoordinates.y, fromCoordinates.x]) ? 'K' : 'k';
					if (Validation.AllCheck(testCoordinates, tempChessBoard).Count > 0)
					{
						return false;
					}
					else if (testCoordinates.x == endPosition)
					{
						howMove = directionX == 1 ? EnumMoves.KingCastle : EnumMoves.QueenCastle;
						return true;
					}
					else if (chessBoard[testCoordinates.y, testCoordinates.x] == '1' || char.ToUpper(chessBoard[testCoordinates.y, testCoordinates.x]) == 'M')
					{
						continue;
					}
					else if (char.ToUpper(chessBoard[testCoordinates.y, testCoordinates.x]) == 'H' && numOfRooks == 0)
					{
						numOfRooks++;
						continue;
					}
					else
					{
						return false;
					}
				}
			}
			return false;
		}

		public static bool CanEscape(char[,] chessBoard, (int x, int y) coordinates)
		{
			bool team = char.IsUpper(chessBoard[coordinates.y, coordinates.x]);
			for (int i = 0; i < 8; i++)
			{
				(int x, int y) testCoordinate = (Mathf.RoundToInt((float)Math.Sin(i * Math.PI * 0.25)) + coordinates.x, Mathf.RoundToInt((float)Math.Cos(i * Math.PI * 0.25)) + coordinates.y);
				if (Validation.CheckBounds(testCoordinate.x, testCoordinate.y)) continue;
				char attackPiece = chessBoard[testCoordinate.y, testCoordinate.x];
				char[,] tempChessBoard = (char[,])chessBoard.Clone();
				tempChessBoard[coordinates.y, coordinates.x] = '1';
				if (Validation.AllCheck(testCoordinate, tempChessBoard, team).Count > 0) continue;
				if (char.IsUpper(attackPiece) != char.IsUpper(chessBoard[coordinates.y, coordinates.x]) || attackPiece == '1') return true;
			}
			return false;
		}
	}
}
