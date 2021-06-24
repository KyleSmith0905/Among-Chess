/*
 * | Base |
 * 1 = Empty
 * pP = Moved Pawn
 * nN = Knight
 * bB = Bishop
 * rR = Moved Rook
 * qQ = Queen
 * kK = King
 * 
 * | Special Moves |
 * fF = Unmoved Pawn
 * eE = Recently Moved Pawn
 * hH = Unmoved Rook
 * mM = Unmoved King
 * 
 * | Variants |
 * 0 = Hole
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmongChess.Patches
{
	class ChessControl
	{
		public static char[] FromTranslate = new char[] { 'f', 'F', 'e', 'E', 'h', 'H', 'm', 'M' };
		public static char[] ToTranslate = new char[] { 'p', 'P', 'p', 'P', 'r', 'R', 'k', 'K' };
		public static string GameMode = null;
		public static string Variant = null;
		public static string Board = null;
		public static string MainTime = null;
		public static string IncrementTime = null;
		public static (int black, int white) numCaptures = (0, 0);
		private static char[,] _ChessBoard = new char[,] { { '0' } };
		public static char[,] ChessBoard
		{
			get
			{
				if (_ChessBoard.Length > 1) return _ChessBoard;
				switch (Board)
				{
					case "Chess960":
					{
						char[,] board = new char[8, 8] {
							{ 'n', 'n', 'n', 'n', 'n', 'n', 'n', 'n' },
							{ 'f', 'f', 'f', 'f', 'f', 'f', 'f', 'f' },
							{ '1', '1', '1', '1', '1', '1', '1', '1' },
							{ '1', '1', '1', '1', '1', '1', '1', '1' },
							{ '1', '1', '1', '1', '1', '1', '1', '1' },
							{ '1', '1', '1', '1', '1', '1', '1', '1' },
							{ 'F', 'F', 'F', 'F', 'F', 'F', 'F', 'F' },
							{ 'N', 'N', 'N', 'N', 'N', 'N', 'N', 'N' }
						};
						for (int i = 0; i < 8; i += 7)
						{
							int kingPosition = UnityEngine.Random.RandomRangeInt(1, 7);
							board[i, kingPosition] = i == 0 ? 'm' : 'M';
							board[i, UnityEngine.Random.RandomRangeInt(0, kingPosition)] = i == 0 ? 'h' : 'H';
							board[i, UnityEngine.Random.RandomRangeInt(kingPosition + 1, 8)] = i == 0 ? 'h' : 'H';
							for (int j = 0; j < 3; j++)
							{
								while (true)
								{
									int position = j == 2 ? UnityEngine.Random.RandomRangeInt(0, 8) : (UnityEngine.Random.RandomRangeInt(0, 4) * 2) + j;
									if (char.ToUpper(board[i, position]) == 'N')
									{
										board[i, position] = j == 2 ? (i == 0 ? 'q' : 'Q') : (i == 0 ? 'b' : 'B');
										break;
									}
								}
							}
						}
						_ChessBoard = board;
						return board;
					}
					default:
					{
						char[,] board = new char[8, 8] {
							{ 'h', 'n', 'b', 'q', 'm', 'b', 'n', 'h' },
							{ 'f', 'f', 'f', 'f', 'f', 'f', 'f', 'f' },
							{ '1', '1', '1', '1', '1', '1', '1', '1' },
							{ '1', '1', '1', '1', '1', '1', '1', '1' },
							{ '1', '1', '1', '1', '1', '1', '1', '1' },
							{ '1', '1', '1', '1', '1', '1', '1', '1' },
							{ 'F', 'F', 'F', 'F', 'F', 'F', 'F', 'F' },
							{ 'H', 'N', 'B', 'Q', 'M', 'B', 'N', 'H' }
						};
						_ChessBoard = board;
						return board;
					}
				}
			}
			set => _ChessBoard = value;
		}

		public static void SetSettings()
		{
			for (int i = 0; i < OptionControl.AllOption.Count; i++)
			{
				OptionSingle optionSingle = OptionControl.AllOption[i];
				string value = optionSingle.AllValues[optionSingle.Value];
				switch (optionSingle.Id)
				{
					case 0: GameMode = value; break;
					case 1: Variant = value; break;
					case 2: Board = value; break;
					case 3: MainTime = value; break;
					case 4: IncrementTime = value; break;
				}
			}
		}

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

		public static char[,] ReadableBoard()
		{
			return ReadableBoard(ChessBoard);
		}

		public static char MovePiece((int x, int y) fromCoordinates, (int x, int y) toCoordinates, GameObject fromObject)
		{
			/*
			Debug.logger.Log("--------");
			for (int y = 0; y < 8; y++)
			{
					string row = "";
					for (int x = 0; x < 8; x++)
					{
							row += ChessBoard[y, x];
					}
					Debug.logger.Log(row);
			}
			*/
			char[,] tempChessBoard = (char[,])ChessBoard.Clone();
			char fromPiece = tempChessBoard[fromCoordinates.y, fromCoordinates.x];
			char toPiece = tempChessBoard[toCoordinates.y, toCoordinates.x];
			if (toPiece != '1' && char.IsUpper(fromPiece) == char.IsUpper(toPiece) && (char.ToUpper(fromPiece) != 'M' || char.ToUpper(toPiece) != 'H')) return 'e';
			bool canMove = false;
			char howMove = 'N';
			switch (char.ToUpper(fromPiece))
			{
				case 'M':
				case 'K':
					canMove = KingMovement(fromCoordinates, toCoordinates, out howMove);
					break;
				case 'Q':
					canMove = QueenMovement(fromCoordinates, toCoordinates);
					break;
				case 'H':
				case 'R':
					canMove = RookMovement(fromCoordinates, toCoordinates);
					break;
				case 'B':
					canMove = BishopMovement(fromCoordinates, toCoordinates);
					break;
				case 'N':
					canMove = KnightMovement(fromCoordinates, toCoordinates);
					break;
				case 'F':
				case 'E':
				case 'P':
					canMove = PawnMovement(fromCoordinates, toCoordinates, out howMove);
					break;
			}
			if (fromObject.name[0] != 't' || !canMove) return 'e';
			_ = PieceAging(ref tempChessBoard);
			PlayMove(ref tempChessBoard, fromCoordinates, toCoordinates, howMove);
			(int x, int y) ourKingCoordinates = KingFinder(char.IsUpper(fromPiece), tempChessBoard);
			(int x, int y) theirKingCoordinates = KingFinder(!char.IsUpper(fromPiece), tempChessBoard);
			List<(int x, int y)> theirKingChecks = NumCheck(theirKingCoordinates, tempChessBoard);
			List<(int x, int y)> ourKingChecks = NumCheck(ourKingCoordinates, tempChessBoard);
			if (ourKingChecks.Count > 0) return 'e';
			if (toPiece != '1' || howMove == 'E')
			{
				GameObject toObject = PlayerControl.LocalPlayer.gameObject;
				Transform piecesObject = GameObject.Find("PiecesPath").transform;
				for (int i = 0; i < piecesObject.childCount; i++)
				{
					Transform elementObject = piecesObject.GetChild(i);
					int pieceNameIndex = elementObject.name.IndexOf(':');
					(int x, int y) testCoordinates = howMove == 'E' ? (toCoordinates.x, fromCoordinates.y) : toCoordinates;
					if (elementObject.name[(pieceNameIndex + 1)..] == testCoordinates.x + "," + testCoordinates.y)
					{
						toObject = elementObject.gameObject;
						break;
					}
				}
				GameEvents.PlayMove(fromObject, toCoordinates, toObject, howMove, char.IsUpper(fromPiece) ? numCaptures.black++ : numCaptures.white++);
			}
			else
			{
				GameEvents.PlayMove(fromObject, toCoordinates, howMove);
			}
			ChessBoard = tempChessBoard;
			return IsInCheckmate(theirKingCoordinates, tempChessBoard) == 'n' ? 'C' : (theirKingChecks.Count == 0 && IsInStalemate(theirKingCoordinates, tempChessBoard) ? 'S' : 'n');
		}

		public static bool KingMovement((int x, int y) fromCoordinates, (int x, int y) toCoordinates, out char howMove)
		{
			howMove = 'N';
			for (int index = 0; index < 8; index++)
			{
				if (char.ToUpper(ChessBoard[toCoordinates.y, toCoordinates.x]) == 'H') break;
				int directionY = Mathf.RoundToInt((float)Math.Sin(index * Math.PI * 0.25));
				int directionX = Mathf.RoundToInt((float)Math.Cos(index * Math.PI * 0.25));
				(int x, int y) coordinate = (x: directionX + fromCoordinates.x, y: directionY + fromCoordinates.y);
				if (CheckBounds(coordinate)) continue;
				else if (coordinate.x == toCoordinates.x && coordinate.y == toCoordinates.y) return true;
			}
			if (char.ToUpper(ChessBoard[fromCoordinates.y, fromCoordinates.x]) == 'M' && char.ToUpper(ChessBoard[toCoordinates.y, toCoordinates.x]) == 'H' && fromCoordinates.y == toCoordinates.y)
			{
				int directionX = Math.Sign(toCoordinates.x - fromCoordinates.x);
				int endPosition = directionX == 1 ? 6 : 2;
				int numOfRooks = 0;
				bool ifOne = ChessBoard[fromCoordinates.y, endPosition] != '1' && endPosition != fromCoordinates.x && endPosition != toCoordinates.x;
				bool ifTwo = ChessBoard[fromCoordinates.y, endPosition - directionX] != '1' && endPosition - directionX != fromCoordinates.x && endPosition - directionX != toCoordinates.x;
				if (ifOne || ifTwo) return false;
				for (int i = 0; i < 8; i++)
				{
					(int x, int y) testCoordinates = (fromCoordinates.x + (directionX * i), fromCoordinates.y);
					if (CheckBounds(testCoordinates)) return false;
					char[,] tempChessBoard = (char[,])ChessBoard.Clone();
					tempChessBoard[fromCoordinates.y, fromCoordinates.x] = '1';
					tempChessBoard[testCoordinates.y, testCoordinates.x] = char.IsUpper(ChessBoard[fromCoordinates.y, fromCoordinates.x]) ? 'K' : 'k';
					if (NumCheck(testCoordinates, tempChessBoard).Count > 0)
					{
						return false;
					}
					else if (testCoordinates.x == endPosition)
					{
						howMove = directionX == 1 ? 'K' : 'Q';
						return true;
					}
					else if (ChessBoard[testCoordinates.y, testCoordinates.x] == '1' || char.ToUpper(ChessBoard[testCoordinates.y, testCoordinates.x]) == 'M')
					{
						continue;
					}
					else if (char.ToUpper(ChessBoard[testCoordinates.y, testCoordinates.x]) == 'H' && numOfRooks == 0)
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

		public static bool QueenMovement((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			return RookMovement(fromCoordinates, toCoordinates) || BishopMovement(fromCoordinates, toCoordinates);
		}

		public static bool RookMovement((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			for (int index = 0; index < 4; index++)
			{
				int directionY = Mathf.RoundToInt((float)Math.Sin(index * Math.PI * 0.5));
				int directionX = Mathf.RoundToInt((float)Math.Cos(index * Math.PI * 0.5));
				for (int i = 1; i < 8; i++)
				{
					(int x, int y) coordinate = ((directionX * i) + fromCoordinates.x, (directionY * i) + fromCoordinates.y);
					if (CheckBounds(coordinate)) break;
					else if (coordinate.x == toCoordinates.x && coordinate.y == toCoordinates.y) return true;
					if (ChessBoard[coordinate.y, coordinate.x] == '1') continue;
					else break;
				}
			}
			return false;
		}

		public static bool BishopMovement((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			for (int index = 0; index < 4; index++)
			{
				int directionY = Mathf.RoundToInt((float)Math.Sin((index * Math.PI * 0.5) + (Math.PI * 0.25)));
				int directionX = Mathf.RoundToInt((float)Math.Cos((index * Math.PI * 0.5) + (Math.PI * 0.25)));
				for (int i = 1; i < 8; i++)
				{
					(int x, int y) coordinate = ((directionX * i) + fromCoordinates.x, (directionY * i) + fromCoordinates.y);
					if (CheckBounds(coordinate)) break;
					else if (coordinate.x == toCoordinates.x && coordinate.y == toCoordinates.y) return true;
					if (ChessBoard[coordinate.y, coordinate.x] == '1') continue;
					else break;
				}
			}
			return false;
		}

		public static bool KnightMovement((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
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

		public static bool PawnMovement((int x, int y) fromCoordinates, (int x, int y) toCoordinates, out char howMove)
		{
			howMove = 'N';
			int directionY = char.IsUpper(ChessBoard[fromCoordinates.y, fromCoordinates.x]) ? -1 : 1;
			char pawnType = ChessBoard[fromCoordinates.y, fromCoordinates.x];
			float halfRank = ChessBoard.GetLength(0) * 0.5f;
			if (toCoordinates.y == halfRank + (halfRank * directionY))
			{
				howMove = 'P';
			}
			if (fromCoordinates.x == toCoordinates.x && fromCoordinates.y + directionY == toCoordinates.y && !CheckBounds(toCoordinates))
			{
				return ChessBoard[toCoordinates.y, toCoordinates.x] == '1';
			}
			else if (fromCoordinates.x == toCoordinates.x && fromCoordinates.y + (directionY * 2) == toCoordinates.y && char.ToUpper(pawnType) == 'F' && !CheckBounds(toCoordinates))
			{
				return ChessBoard[fromCoordinates.y + directionY, toCoordinates.x] == '1' && ChessBoard[toCoordinates.y, toCoordinates.x] == '1';
			}
			for (int directionX = -1; directionX < 2; directionX += 2)
			{
				(int x, int y) testCoordinates = (fromCoordinates.x + directionX, fromCoordinates.y + directionY);
				if (testCoordinates.x == toCoordinates.x && testCoordinates.y == toCoordinates.y && !CheckBounds(testCoordinates) && ChessBoard[testCoordinates.y, testCoordinates.x] != '1') return true;
			}
			for (int directionX = -1; directionX < 2; directionX += 2)
			{
				(int x, int y) testCoordinates = (fromCoordinates.x + directionX, fromCoordinates.y + directionY);
				if (testCoordinates.x == toCoordinates.x && testCoordinates.y == toCoordinates.y && !CheckBounds(toCoordinates) && ChessBoard[toCoordinates.y, toCoordinates.x] == '1' && char.ToUpper(ChessBoard[fromCoordinates.y, toCoordinates.x]) == 'E')
				{
					howMove = 'E';
					return true;
				}
			}
			return false;
		}

		public static List<(int x, int y)> NumCheck((int x, int y) kingCoordinates, char[,] chessBoard, bool color)
		{
			List<(int x, int y)> possibleMoves = new List<(int x, int y)> { };
			char[,] tempChessBoard = ReadableBoard(chessBoard);
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

		public static List<(int x, int y)> NumCheck((int x, int y) kingCoordinates, char[,] chessBoard)
		{
			return NumCheck(kingCoordinates, chessBoard, char.IsUpper(chessBoard[kingCoordinates.y, kingCoordinates.x]));
		}

		public static char IsInCheckmate((int x, int y) kingCoordinates, char[,] chessBoard)
		{
			List<(int x, int y)> checks = NumCheck(kingCoordinates, chessBoard);
			if (checks.Count == 0) return 'z';
			for (int i = 0; i < 8; i++)
			{
				(int x, int y) escapeCoordinates = (Mathf.RoundToInt((float)Math.Cos(i * Math.PI * 0.25)) + kingCoordinates.x, Mathf.RoundToInt((float)Math.Sin(i * Math.PI * 0.25)) + kingCoordinates.y);
				if (CheckBounds(escapeCoordinates) || chessBoard[escapeCoordinates.y, escapeCoordinates.x] != '1') continue;
				List<(int x, int y)> numChecks = NumCheck(escapeCoordinates, chessBoard, char.IsUpper(chessBoard[kingCoordinates.y, kingCoordinates.x]));
				if (numChecks.Count == 0) return 'e';
			}
			if (checks.Count > 1) return 'n';
			List<(int x, int y)> captures = NumCheck((checks[0].x, checks[0].y), chessBoard);
			for (int index = 0; index < captures.Count; index++)
			{
				char piece = chessBoard[captures[0].y, captures[0].x];
				if (piece == 'N') return 'c';
				(int x, int y) kingDirection = (Math.Sign(captures[index].x - kingCoordinates.x), Math.Sign(captures[index].y - kingCoordinates.y));
				for (int i = 0; i < 6; i++)
				{
					(int x, int y) coordinates = ((kingDirection.x * i) + captures[index].x, (kingDirection.y * i) + captures[index].y);
					if (CheckBounds(coordinates)) break;
					char holdingPiece = chessBoard[coordinates.y, coordinates.x];
					char offensePiece = Math.Abs(kingDirection.x - kingDirection.y) == 1 ? 'B' : 'R';
					if (holdingPiece == '1') continue;
					else if ((char.IsUpper(holdingPiece) != char.IsUpper(piece)) && (char.ToUpper(holdingPiece) == offensePiece || char.ToUpper(holdingPiece) == offensePiece)) break;
					else return 'c';
				}
			}
			char attackPiece = chessBoard[checks[0].y, checks[0].x];
			if (attackPiece == 'P' || attackPiece == 'N' || attackPiece == 'K') return 'n';
			(int x, int y) direction = (Math.Sign(checks[0].x - kingCoordinates.x), Math.Sign(checks[0].y - kingCoordinates.y));
			for (int index = 0; index < 6; index++)
			{
				(int x, int y) testCoordinates = ((direction.x * index) + kingCoordinates.x, (direction.y * index) + kingCoordinates.y);
				if (CheckBounds(testCoordinates)) return 'n';
				List<(int x, int y)> blocks = NumCheck(testCoordinates, chessBoard, !char.IsUpper(chessBoard[kingCoordinates.y, kingCoordinates.x]));
				for (int ind = 0; ind < blocks.Count; ind++)
				{
					if (char.ToUpper(chessBoard[blocks[ind].y, blocks[ind].x]) == 'K' || char.ToUpper(chessBoard[blocks[ind].y, blocks[ind].x]) == 'M') continue;
					(int x, int y) defendDirection = (Math.Sign(blocks[ind].x - kingCoordinates.x), Math.Sign(blocks[ind].y - kingCoordinates.y));
					for (int i = 0; i < 6; i++)
					{
						(int x, int y) coordinates = ((defendDirection.x * i) + blocks[ind].x, (defendDirection.y * i) + blocks[ind].y);
						if (CheckBounds(coordinates)) break;
						char holdingPiece = chessBoard[coordinates.y, coordinates.x];
						char offensePiece = Math.Abs(defendDirection.x - defendDirection.y) == 1 ? 'B' : 'R';
						if (holdingPiece == '1') continue;
						else if ((char.IsUpper(holdingPiece) == char.IsUpper(attackPiece)) && char.ToUpper(holdingPiece) != offensePiece) break;
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
			char piece = chessBoard[coordinates.y, coordinates.x];
			bool pieceMove = false;
			switch (char.ToUpper(piece))
			{
				case 'F':
				case 'E':
				case 'P':
					int directionY = char.IsUpper(piece) ? -1 : 1;
					if (!CheckBounds(coordinates.x, directionY + coordinates.y) && chessBoard[directionY + coordinates.y, coordinates.x] == '1') { pieceMove = true; break; }
					else if (!CheckBounds(coordinates.x, (directionY * 2) + coordinates.y) && char.ToUpper(piece) == 'F' && char.ToUpper(chessBoard[(directionY * 2) + coordinates.y, coordinates.x]) == '1') { pieceMove = true; break; }
					else
					{
						for (int i = -1; i < 2; i += 2)
						{
							if ((!CheckBounds(coordinates.x + i, directionY + coordinates.y) && chessBoard[directionY + coordinates.y, coordinates.x + i] != '1' && char.IsUpper(chessBoard[directionY + coordinates.y, coordinates.x + i]) != char.IsUpper(piece)) || (!CheckBounds(coordinates.x + i, coordinates.y) && chessBoard[coordinates.y, coordinates.x + i] == 'E')) { pieceMove = true; break; }
						}
					}
					break;
				case 'N':
					int[] moveX = new int[] { -2, -1, -2, -1, 2, 1, 2, 1 };
					int[] moveY = new int[] { -1, -2, 1, 2, -1, -2, 1, 2 };
					for (int i = 0; i < 8; i++)
					{
						(int x, int y) testCoordinate = (moveX[i] + coordinates.x, moveY[i] + coordinates.y);
						if (CheckBounds(testCoordinate.x, testCoordinate.y)) continue;
						char attackPiece = chessBoard[testCoordinate.y, testCoordinate.x];
						if (attackPiece == '1' || char.IsUpper(attackPiece) != char.IsUpper(piece)) { pieceMove = true; break; }
					}
					break;
				case 'B':
					for (int i = 0; i < 4; i++)
					{
						(int x, int y) testCoordinate = (Mathf.RoundToInt((float)Math.Sin((i * Math.PI * 0.5) + (Math.PI * 0.25))) + coordinates.x, Mathf.RoundToInt((float)Math.Cos((i * Math.PI * 0.5) + (Math.PI * 0.25))) + coordinates.y);
						if (CheckBounds(testCoordinate.x, testCoordinate.y)) continue;
						char attackPiece = chessBoard[testCoordinate.y, testCoordinate.x];
						if (char.IsUpper(attackPiece) != char.IsUpper(piece) || attackPiece == '1') { pieceMove = true; break; }
					}
					break;
				case 'H':
				case 'R':
					for (int i = 0; i < 4; i++)
					{
						(int x, int y) testCoordinate = (Mathf.RoundToInt((float)Math.Sin(i * Math.PI * 0.5)) + coordinates.x, Mathf.RoundToInt((float)Math.Cos(i * Math.PI * 0.5)) + coordinates.y);
						if (CheckBounds(testCoordinate.x, testCoordinate.y)) continue;
						char attackPiece = chessBoard[testCoordinate.y, testCoordinate.x];
						if (char.IsUpper(attackPiece) != char.IsUpper(piece) || attackPiece == '1') { pieceMove = true; break; }
					}
					break;
				case 'Q':
				case 'M':
				case 'K':
					for (int i = 0; i < 8; i++)
					{
						(int x, int y) testCoordinate = (Mathf.RoundToInt((float)Math.Sin(i * Math.PI * 0.25)) + coordinates.x, Mathf.RoundToInt((float)Math.Cos(i * Math.PI * 0.25)) + coordinates.y);
						if (CheckBounds(testCoordinate.x, testCoordinate.y)) continue;
						char attackPiece = chessBoard[testCoordinate.y, testCoordinate.x];
						if (char.IsUpper(attackPiece) != char.IsUpper(piece) || attackPiece == '1') { pieceMove = true; break; }
					}
					break;
			}
			if (pieceMove == true)
			{
				char[,] tempChessBoard = (char[,])chessBoard.Clone();
				tempChessBoard[coordinates.y, coordinates.x] = '1';
				if (NumCheck(kingCoordinates, tempChessBoard).Count == 0) return true;
			}
			return false;
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

		public static bool CheckBounds((int x, int y) coordinates)
		{
			return coordinates.x < 0 || coordinates.y < 0 || coordinates.x >= ChessBoard.GetLength(0) || coordinates.y >= ChessBoard.GetLength(1);
		}

		public static bool CheckBounds(int coordinateX, int coordinateY)
		{
			return CheckBounds((x: coordinateX, y: coordinateY));
		}

		public static void PlayMove(ref char[,] chessBoard, (int x, int y) fromCoordinates, (int x, int y) toCoordinates, char howMove)
		{
			char fromPiece = chessBoard[fromCoordinates.y, fromCoordinates.x];
			char toPiece = chessBoard[toCoordinates.y, toCoordinates.x];
			char[] fromTranslate = new char[] { 'f', 'F', 'h', 'H', 'm', 'M' };
			char[] toTranslate = new char[] { 'e', 'E', 'r', 'R', 'k', 'K' };
			int index = Array.IndexOf(fromTranslate, fromPiece);
			if (index != -1) fromPiece = toTranslate[index];
			if (howMove == 'P') fromPiece = 'Q';
			switch (howMove)
			{
				case 'K':
				case 'Q':
					if (toPiece == 'h') toPiece = 'r';
					else if (toPiece == 'H') toPiece = 'R';
					chessBoard[fromCoordinates.y, fromCoordinates.x] = '1';
					chessBoard[fromCoordinates.y, toCoordinates.x] = '1';
					chessBoard[fromCoordinates.y, howMove == 'Q' ? 2 : 6] = fromPiece;
					chessBoard[fromCoordinates.y, howMove == 'Q' ? 3 : 5] = toPiece;
					break;
				case 'E':
					chessBoard[toCoordinates.y, toCoordinates.x] = fromPiece;
					chessBoard[fromCoordinates.y, toCoordinates.x] = '1';
					chessBoard[fromCoordinates.y, fromCoordinates.x] = '1';
					break;
				default:
					chessBoard[toCoordinates.y, toCoordinates.x] = fromPiece;
					chessBoard[fromCoordinates.y, fromCoordinates.x] = '1';
					break;
			}
		}

		public static char[,] PlayMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			char[,] tempChessBoard = (char[,])ChessBoard.Clone();
			char howMove = GetHowMove(fromCoordinates, toCoordinates);
			PlayMove(ref tempChessBoard, fromCoordinates, toCoordinates, howMove);
			return tempChessBoard;
		}

		public static char GetHowMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			char howMove = 'N';
			if (char.ToUpper(ChessBoard[fromCoordinates.y, fromCoordinates.x]) == 'M' && char.ToUpper(ChessBoard[toCoordinates.y, toCoordinates.x]) == 'H' && fromCoordinates.y == toCoordinates.y)
			{
				howMove = Math.Sign(toCoordinates.x - fromCoordinates.x) == 1 ? 'K' : 'Q';
			}
			else if (char.ToUpper(ReadablePiece(ChessBoard[fromCoordinates.y, fromCoordinates.x])) == 'P' && char.ToUpper(ChessBoard[fromCoordinates.y, toCoordinates.x]) == 'E')
			{
				howMove = 'E';
			}
			return howMove;
		}

		public static char[,] PieceAging(ref char[,] chessBoard)
		{
			for (int y = 0; y < chessBoard.GetLength(0); y++)
			{
				for (int x = 0; x < chessBoard.GetLength(1); x++)
				{
					if (char.ToUpper(chessBoard[y, x]) == 'E') chessBoard.SetValue(char.IsUpper(chessBoard[y, x]) ? 'P' : 'p', new int[] { y, x });
				}
			}
			return chessBoard;
		}
	}
}