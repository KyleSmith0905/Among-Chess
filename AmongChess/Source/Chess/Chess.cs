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

namespace AmongChess.Source.Chess
{
	internal class Chess
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
				char[,] board = new char[8, 8]{
					{ 'h', 'n', 'b', 'q', 'm', 'b', 'n', 'h' },
					{ 'f', 'f', 'f', 'f', 'f', 'f', 'f', 'f' },
					{ '1', '1', '1', '1', '1', '1', '1', '1' },
					{ '1', '1', '1', '1', '1', '1', '1', '1' },
					{ '1', '1', '1', '1', '1', '1', '1', '1' },
					{ '1', '1', '1', '1', '1', '1', '1', '1' },
					{ 'F', 'F', 'F', 'F', 'F', 'F', 'F', 'F' },
					{ 'H', 'N', 'B', 'Q', 'M', 'B', 'N', 'H' }
				};
				switch (Board)
				{
					case "Chess960":
					{
						board = Settings.Chess960.GetBoard();
						break;
					}
					case "Transcendental":
					{
						board = Settings.Transcendental.GetBoard();
						break;
					}
				}
				_ChessBoard = board;
				return board;
			}
			set => _ChessBoard = value;
		}

		public static void SetSettings()
		{
			for (int i = 0; i < Lobby.Options.AllOption.Count; i++)
			{
				Lobby.ClassOption optionSingle = Lobby.Options.AllOption[i];
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
			switch (char.ToUpper(Utils.ReadablePiece(fromPiece)))
			{
				case 'K':
					canMove = Pieces.King.CanMove(fromCoordinates, toCoordinates, ref howMove);
					break;
				case 'Q':
					canMove = Pieces.Queen.CanMove(fromCoordinates, toCoordinates);
					break;
				case 'R':
					canMove = Pieces.Rook.CanMove(fromCoordinates, toCoordinates);
					break;
				case 'B':
					canMove = Pieces.Bishop.CanMove(fromCoordinates, toCoordinates);
					break;
				case 'N':
					canMove = Pieces.Knight.CanMove(fromCoordinates, toCoordinates);
					break;
				case 'P':
					canMove = Pieces.Pawn.CanMove(fromCoordinates, toCoordinates, ref howMove);
					break;
			}
			if (fromObject.name[0] != 't' || !canMove) return EnumResults.ErrorInvalid;
			tempChessBoard = ChessAging(tempChessBoard);
			PlayMove(ref tempChessBoard, fromCoordinates, toCoordinates, howMove);
			if (char.ToUpper(Utils.ReadablePiece(toPiece)) == 'K') return EnumResults.WinCheckmate;
			(int x, int y) ourKingCoordinates = Utils.KingFinder(char.IsUpper(fromPiece), tempChessBoard);
			(int x, int y) theirKingCoordinates = Utils.KingFinder(!char.IsUpper(fromPiece), tempChessBoard);
			List<(int x, int y)> ourKingChecks = Validation.AllCheck(ourKingCoordinates, tempChessBoard);
			List<(int x, int y)> theirKingChecks = Validation.AllCheck(theirKingCoordinates, tempChessBoard);
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
				Game.Game.PlayMove(fromObject, toCoordinates, toObject, howMove, char.IsUpper(fromPiece) ? numCaptures.black++ : numCaptures.white++);
			}
			else
			{
				Game.Game.PlayMove(fromObject, toCoordinates, howMove);
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
			if (howMove == EnumMoves.Promotion) fromPiece = char.IsUpper(fromPiece) ? 'Q' : 'q';
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
			EnumMoves howMove = Utils.GetHowMove(fromCoordinates, toCoordinates);
			PlayMove(ref tempChessBoard, fromCoordinates, toCoordinates, howMove);
			return tempChessBoard;
		}

		public static char[,] ChessAging(char[,] chessBoard)
		{
			for (int y = 0; y < chessBoard.GetLength(0); y++)
			{
				for (int x = 0; x < chessBoard.GetLength(1); x++)
				{
					char piece = PieceAging(chessBoard[y, x]);
					if (piece != chessBoard[y, x]) chessBoard.SetValue(piece, new int[] { y, x });
				}
			}
			return chessBoard;
		}

		public static char PieceAging(char piece)
		{
			return char.ToUpper(piece) == 'E' ? char.IsUpper(piece) ? 'P' : 'p' : piece;
		}
	}
}