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

namespace AmongChess.Patches.ChessControl
{
	internal class Control
	{
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
			for (int i = 0; i < LobbyControl.OptionControl.AllOption.Count; i++)
			{
				OptionSingle optionSingle = LobbyControl.OptionControl.AllOption[i];
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

		public static EnumResults MovePiece((int x, int y) fromCoordinates, (int x, int y) toCoordinates, GameObject fromObject)
		{
			char[,] tempChessBoard = (char[,])ChessBoard.Clone();
			char fromPiece = tempChessBoard[fromCoordinates.y, fromCoordinates.x];
			char toPiece = tempChessBoard[toCoordinates.y, toCoordinates.x];
			if (toPiece != '1' && char.IsUpper(fromPiece) == char.IsUpper(toPiece) && (char.ToUpper(fromPiece) != 'M' || char.ToUpper(toPiece) != 'H')) return EnumResults.ErrorInvalid;
			bool canMove = false;
			EnumMoves howMove = EnumMoves.Normal;
			switch (char.ToUpper(Utilities.ReadablePiece(fromPiece)))
			{
				case 'K':
					canMove = ChessPieces.King.CanMove(fromCoordinates, toCoordinates, ref howMove);
					break;
				case 'Q':
					canMove = ChessPieces.Queen.CanMove(fromCoordinates, toCoordinates);
					break;
				case 'R':
					canMove = ChessPieces.Rook.CanMove(fromCoordinates, toCoordinates);
					break;
				case 'B':
					canMove = ChessPieces.Bishop.CanMove(fromCoordinates, toCoordinates);
					break;
				case 'N':
					canMove = ChessPieces.Knight.CanMove(fromCoordinates, toCoordinates);
					break;
				case 'P':
					canMove = ChessPieces.Pawn.CanMove(fromCoordinates, toCoordinates, ref howMove);
					break;
			}
			if (fromObject.name[0] != 't' || !canMove) return EnumResults.ErrorInvalid;
			PieceAging(ref tempChessBoard);
			PlayMove(ref tempChessBoard, fromCoordinates, toCoordinates, howMove);
			(int x, int y) ourKingCoordinates = Utilities.KingFinder(char.IsUpper(fromPiece), tempChessBoard);
			(int x, int y) theirKingCoordinates = Utilities.KingFinder(!char.IsUpper(fromPiece), tempChessBoard);
			List<(int x, int y)> ourKingChecks = Validation.NumCheck(ourKingCoordinates, tempChessBoard);
			List<(int x, int y)> theirKingChecks = Validation.NumCheck(theirKingCoordinates, tempChessBoard);
			if (ourKingChecks.Count > 0) return EnumResults.ErrorInvalid;
			if (toPiece != '1' || howMove == EnumMoves.EnPassant)
			{
				GameObject toObject = PlayerControl.LocalPlayer.gameObject;
				Transform piecesObject = GameObject.Find("PiecesPath").transform;
				for (int i = 0; i < piecesObject.childCount; i++)
				{
					Transform elementObject = piecesObject.GetChild(i);
					int pieceNameIndex = elementObject.name.IndexOf(':');
					(int x, int y) testCoordinates = howMove == EnumMoves.EnPassant ? (toCoordinates.x, fromCoordinates.y) : toCoordinates;
					if (elementObject.name[(pieceNameIndex + 1)..] == testCoordinates.x + "," + testCoordinates.y)
					{
						toObject = elementObject.gameObject;
						break;
					}
				}
				GameControl.Control.PlayMove(fromObject, toCoordinates, toObject, howMove, char.IsUpper(fromPiece) ? numCaptures.black++ : numCaptures.white++);
			}
			else
			{
				GameControl.Control.PlayMove(fromObject, toCoordinates, howMove);
			}
			ChessBoard = tempChessBoard;
			if (Validation.IsInCheckmate(theirKingCoordinates, tempChessBoard) == 'n') return EnumResults.WinCheckmate;
			else if (theirKingChecks.Count == 0 && Validation.IsInStalemate(theirKingCoordinates, tempChessBoard)) return EnumResults.DrawStalemate;
			return EnumResults.MoveNormal;
		}

		public static void PlayMove(ref char[,] chessBoard, (int x, int y) fromCoordinates, (int x, int y) toCoordinates, EnumMoves howMove)
		{
			char fromPiece = chessBoard[fromCoordinates.y, fromCoordinates.x];
			char toPiece = chessBoard[toCoordinates.y, toCoordinates.x];
			char[] fromTranslate = new char[] { 'f', 'F', 'h', 'H', 'm', 'M' };
			char[] toTranslate = new char[] { 'e', 'E', 'r', 'R', 'k', 'K' };
			int index = Array.IndexOf(fromTranslate, fromPiece);
			if (index != -1) fromPiece = toTranslate[index];
			if (howMove == EnumMoves.Promotion) fromPiece = 'Q';
			switch (howMove)
			{
				case EnumMoves.KingCastle:
				case EnumMoves.QueenCastle:
					if (toPiece == 'h') toPiece = 'r';
					else if (toPiece == 'H') toPiece = 'R';
					chessBoard[fromCoordinates.y, fromCoordinates.x] = '1';
					chessBoard[fromCoordinates.y, toCoordinates.x] = '1';
					chessBoard[fromCoordinates.y, howMove == EnumMoves.QueenCastle ? 2 : 6] = fromPiece;
					chessBoard[fromCoordinates.y, howMove == EnumMoves.QueenCastle ? 3 : 5] = toPiece;
					break;
				case EnumMoves.EnPassant:
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
			EnumMoves howMove = Utilities.GetHowMove(fromCoordinates, toCoordinates);
			PlayMove(ref tempChessBoard, fromCoordinates, toCoordinates, howMove);
			return tempChessBoard;
		}

		public static void PieceAging(ref char[,] chessBoard)
		{
			for (int y = 0; y < chessBoard.GetLength(0); y++)
			{
				for (int x = 0; x < chessBoard.GetLength(1); x++)
				{
					if (char.ToUpper(chessBoard[y, x]) == 'E') chessBoard.SetValue(char.IsUpper(chessBoard[y, x]) ? 'P' : 'p', new int[] { y, x });
				}
			}
		}
	}
}