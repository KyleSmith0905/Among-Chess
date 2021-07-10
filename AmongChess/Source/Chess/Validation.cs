using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmongChess.Source.Chess
{
	internal class Validation
	{
		public static List<(int x, int y)> AllCheck((int x, int y) kingCoordinates, char[,] chessBoard, bool color)
		{
			List<(int x, int y)> possibleMoves = new List<(int x, int y)> { };
			char[,] tempChessBoard = Utils.ReadableBoard(chessBoard);
			int[] moveX = new int[] { -2, -1, -2, -1, 2, 1, 2, 1 };
			int[] moveY = new int[] { -1, -2, 1, 2, -1, -2, 1, 2 };
			for (int index = 0; index < 8; index++)
			{
				(int x, int y) coordinate = (moveX[index] + kingCoordinates.x, moveY[index] + kingCoordinates.y);
				if (CheckBounds(coordinate)) continue;
				if (char.ToUpper(tempChessBoard[coordinate.y, coordinate.x]) == 'N' && char.IsUpper(tempChessBoard[coordinate.y, coordinate.x]) != color) possibleMoves.Add((coordinate.x, coordinate.y));
			}
			for (int index = 0; index < 8; index++)
			{
				(int x, int y) direction = (Mathf.RoundToInt((float)Math.Cos(index * Math.PI * 0.25)), Mathf.RoundToInt((float)Math.Sin(index * Math.PI * 0.25)));
				for (int i = 1; i < 8; i++)
				{
					(int x, int y) coordinates = ((direction.x * i) + kingCoordinates.x, (direction.y * i) + kingCoordinates.y);
					if (CheckBounds(coordinates)) break;
					char piece = tempChessBoard[coordinates.y, coordinates.x];
					if (piece == '1') continue;
					else if (char.IsUpper(tempChessBoard[coordinates.y, coordinates.x]) == color) break;
					else if ((char.ToUpper(piece) == 'Q') || (char.ToUpper(piece) == 'R' && index % 2 == 0) || (char.ToUpper(piece) == 'B' && index % 2 == 1) || (char.ToUpper(piece) == 'K' && i == 1)) possibleMoves.Add((coordinates.x, coordinates.y));
					else break;
				}
			}
			int directionY = color ? -1 : 1;
			for (int directionX = -1; directionX < 2; directionX += 2)
			{
				(int x, int y) coordinate = (kingCoordinates.x + directionX, kingCoordinates.y + directionY);
				if (CheckBounds(coordinate) || char.IsUpper(tempChessBoard[coordinate.y, coordinate.x]) == color) continue;
				if (char.ToUpper(tempChessBoard[coordinate.y, coordinate.x]) == 'P') possibleMoves.Add((coordinate.x, coordinate.y));
			}
			return possibleMoves;
		}

		public static List<(int x, int y)> AllCheck((int x, int y) kingCoordinates, char[,] chessBoard)
		{
			return AllCheck(kingCoordinates, chessBoard, char.IsUpper(chessBoard[kingCoordinates.y, kingCoordinates.x]));
		}

		public static char IsInCheckmate((int x, int y) kingCoordinates, char[,] chessBoard)
		{
			List<(int x, int y)> checks = AllCheck(kingCoordinates, chessBoard);
			if (checks.Count == 0) return 'z';
			for (int i = 0; i < 8; i++)
			{
				char[,] tempChessBoard = (char[,])chessBoard.Clone();
				(int x, int y) escapeCoordinates = (Mathf.RoundToInt((float)Math.Cos(i * Math.PI * 0.25)) + kingCoordinates.x, Mathf.RoundToInt((float)Math.Sin(i * Math.PI * 0.25)) + kingCoordinates.y);
				tempChessBoard[kingCoordinates.y, kingCoordinates.x] = '1';
				if (CheckBounds(escapeCoordinates) || (char.IsUpper(chessBoard[escapeCoordinates.y, escapeCoordinates.x]) == char.IsUpper(chessBoard[kingCoordinates.y, kingCoordinates.x]) && chessBoard[escapeCoordinates.y, escapeCoordinates.x] != '1')) continue;
				List<(int x, int y)> numChecks = AllCheck(escapeCoordinates, tempChessBoard, char.IsUpper(chessBoard[kingCoordinates.y, kingCoordinates.x]));
				if (numChecks.Count == 0) return 'e';
			}
			if (checks.Count > 1) return 'n';
			char attackPiece = chessBoard[checks[0].y, checks[0].x];
			List<(int x, int y)> captures = AllCheck(checks[0], chessBoard);
			for (int index = 0; index < captures.Count; index++)
			{
				char piece = chessBoard[captures[0].y, captures[0].x];
				if (char.ToUpper(piece) == 'N') return 'c';
				else if (char.ToUpper(Utils.ReadablePiece(piece)) == 'K') continue;
				(int x, int y) kingDirection = (Math.Sign(captures[index].x - kingCoordinates.x), Math.Sign(captures[index].y - kingCoordinates.y));
				for (int i = 1; i < 7; i++)
				{
					(int x, int y) coordinates = ((kingDirection.x * i) + captures[index].x, (kingDirection.y * i) + captures[index].y);
					if (CheckBounds(coordinates)) return 'c';
					char holdingPiece = chessBoard[coordinates.y, coordinates.x];
					char offensePiece = Math.Abs(kingDirection.x - kingDirection.y) == 1 ? 'B' : 'R';
					if (holdingPiece == '1') continue;
					else if (char.IsUpper(holdingPiece) == char.IsUpper(attackPiece) && char.ToUpper(holdingPiece) != offensePiece) break;
					else return 'c';
				}
			}
			char confirmPiece = char.ToUpper(Utils.ReadablePiece(attackPiece));
			if (confirmPiece == 'P' || confirmPiece == 'N' || confirmPiece == 'K') return 'n';
			(int x, int y) direction = (Math.Sign(checks[0].x - kingCoordinates.x), Math.Sign(checks[0].y - kingCoordinates.y));
			for (int index = 1; index < 6; index++)
			{
				(int x, int y) testCoordinates = ((direction.x * index) + kingCoordinates.x, (direction.y * index) + kingCoordinates.y);
				if (CheckBounds(testCoordinates) || chessBoard[testCoordinates.y, testCoordinates.x] != '1') return 'n';
				List<(int x, int y)> blocks = AllCheck(testCoordinates, chessBoard, !char.IsUpper(chessBoard[kingCoordinates.y, kingCoordinates.x]));
				for (int ind = 0; ind < blocks.Count; ind++)
				{
					if (char.ToUpper(Utils.ReadablePiece(chessBoard[blocks[ind].y, blocks[ind].x])) == 'K') continue;
					(int x, int y) defendDirection = (Math.Sign(blocks[ind].x - kingCoordinates.x), Math.Sign(blocks[ind].y - kingCoordinates.y));
					for (int i = 1; i < 7; i++)
					{
						(int x, int y) coordinates = ((defendDirection.x * i) + blocks[ind].x, (defendDirection.y * i) + blocks[ind].y);
						if (CheckBounds(coordinates)) return 'b';
						char holdingPiece = chessBoard[coordinates.y, coordinates.x];
						char offensePiece = Math.Abs(defendDirection.x - defendDirection.y) == 1 ? 'B' : 'R';
						if (holdingPiece == '1') continue;
						else if (char.IsUpper(holdingPiece) == char.IsUpper(attackPiece) && char.ToUpper(holdingPiece) != offensePiece) break;
						else return 'b';
					}
				}
			}
			return 'n';
		}

		public static bool IsInStalemate((int x, int y) kingCoordinates, char[,] chessBoard)
		{
			char king = chessBoard[kingCoordinates.y, kingCoordinates.x];
			for (int coordinateY = 0; coordinateY < chessBoard.GetLength(0); coordinateY++)
			{
				for (int coordinateX = 0; coordinateX < chessBoard.GetLength(1); coordinateX++)
				{
					char piece = chessBoard[coordinateY, coordinateX];
					if (piece == '1' || char.IsUpper(piece) != char.IsUpper(king)) continue;
					bool canMove = CanMove((coordinateX, coordinateY), kingCoordinates, chessBoard);
					if (canMove == true) return false;
				}
			}
			return true;
		}

		public static bool CanMove((int x, int y) coordinates, (int x, int y) kingCoordinates, char[,] chessBoard)
		{
			bool pieceMove = false;
			char piece = char.ToUpper(Utils.ReadablePiece(chessBoard[coordinates.y, coordinates.x]));
			switch (piece)
			{
				case 'P':
					pieceMove = Pieces.Pawn.CanEscape(chessBoard, coordinates);
					break;
				case 'N':
					pieceMove = Pieces.Knight.CanEscape(chessBoard, coordinates);
					break;
				case 'B':
					pieceMove = Pieces.Bishop.CanEscape(chessBoard, coordinates);
					break;
				case 'R':
					pieceMove = Pieces.Rook.CanEscape(chessBoard, coordinates);
					break;
				case 'Q':
					pieceMove = Pieces.Queen.CanEscape(chessBoard, coordinates);
					break;
				case 'K':
					pieceMove = Pieces.King.CanEscape(chessBoard, coordinates);
					break;
			}
			if (pieceMove == true)
			{
				char[,] tempChessBoard = (char[,])chessBoard.Clone();
				tempChessBoard[coordinates.y, coordinates.x] = '1';
				if (AllCheck(kingCoordinates, tempChessBoard).Count == 0) return true;
			}
			return false;
		}

		public static bool CheckBounds((int x, int y) coordinates)
		{
			return coordinates.x < 0 || coordinates.y < 0 || coordinates.x >= Chess.ChessBoard.GetLength(0) || coordinates.y >= Chess.ChessBoard.GetLength(1);
		}

		public static bool CheckBounds(int coordinateX, int coordinateY)
		{
			return CheckBounds((x: coordinateX, y: coordinateY));
		}
	}
}
