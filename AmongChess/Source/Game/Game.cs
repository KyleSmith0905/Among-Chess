using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AmongChess.Source.Game
{
	internal class Game
	{
		public static List<PlayerControl> AllPlayers = new List<PlayerControl> { };
		public static List<CustomPlayer> AllCustomPlayers = new List<CustomPlayer> { };
		public static int[][] ColorIds = new int[2][] { new int[1] { 1 }, new int[2] { 7, 6 } };
		public static string[] ColorNames = new string[18] { "Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple", "Brown", "Cyan", "Lime", "Maroon", "Rose", "Banana", "Gray", "Tan", "Coral" };
		public static byte PlayerTurn = 0;
		public static uint TotalTurns = 0;
		public static EnumActivity LocalActivity
		{
			get => AllCustomPlayers.Count < 1 ? EnumActivity.Lobby : Utils.FindCustom(PlayerControl.LocalPlayer.PlayerId).Activity;
			set => Utils.FindCustom(PlayerControl.LocalPlayer.PlayerId).Activity = value;
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
		public static class PlayerControlPatch
		{
			public static void Prefix(PlayerControl __instance)
			{
				__instance.Visible = true;
			}
		}

		[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
		public static class OnPlayerLeftPatch
		{
			public static void Postfix()
			{
				EventEnded(Chess.EnumResults.WinResignation, PlayerControl.LocalPlayer.PlayerId, true);
			}
		}

		[HarmonyPatch(typeof(HudManager), nameof(HudManager.ShowTaskComplete))]
		public static class ShowTaskCompletePatch
		{
			public static void Prefix()
			{
				GameObject.Find("Main Camera/Hud/TaskCompleteOverlay_TMP").GetComponent<TextMeshPro>().text = "It's Your Turn";
			}
		}

		public static void EventEnded(Chess.EnumResults results, byte winnerId, bool rpcSend)
		{
			if (rpcSend == true)
			{
				MessageWriter rpcMessageLocal = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 67, (SendOption)1);
				rpcMessageLocal.Write((byte)results);
				rpcMessageLocal.Write(winnerId);
				rpcMessageLocal.EndMessage();
			}
			LocalActivity = EnumActivity.GameEnd;
			int colorId = Utils.FindPlayer(winnerId).Data.ColorId;
			string colorName = ColorNames[colorId].ToString();
			if (results == Chess.EnumResults.DrawStalemate)
			{
				HudManager.Instance.ShowPopUp("The game ended in a draw by stalemate.");
			}
			else if (results == Chess.EnumResults.DrawMaterial)
			{
				HudManager.Instance.ShowPopUp("The game ended in a draw by insufficient material.");
			}
			else if (results == Chess.EnumResults.DrawFifty)
			{
				HudManager.Instance.ShowPopUp("The game ended in a draw by the fifty-move-rule.");
			}
			else if (results == Chess.EnumResults.DrawRepetition)
			{
				HudManager.Instance.ShowPopUp("The game ended in a draw by repetition.");
			}
			else if (results == Chess.EnumResults.DrawAgreement)
			{
				HudManager.Instance.ShowPopUp("The game ended in a draw by agreement.");
			}
			else if (results == Chess.EnumResults.DrawTimeout)
			{
				HudManager.Instance.ShowPopUp("The game ended in a draw by timeout.");
			}
			else if (results == Chess.EnumResults.WinCheckmate)
			{
				HudManager.Instance.ShowPopUp(colorName + " won by checkmate.");
			}
			else if (results == Chess.EnumResults.WinTimeout)
			{
				for (int i = 0; i < AllCustomPlayers.Count; i++) if (winnerId != AllCustomPlayers[i].PlayerId) AllCustomPlayers[i].Timer = 0f;
				HudManager.Instance.ShowPopUp(colorName + " won by timeout.");
			}
			else if (results == Chess.EnumResults.WinResignation)
			{
				HudManager.Instance.ShowPopUp(colorName + " won by resignation.");
			}
			else
			{
				HudManager.Instance.ShowPopUp("The game mysteriously ended.");
			}

			Buttons.ClearAllHighlighted();
			End.WinnerId = (int)results < 16 ? -1 : winnerId;
		}

		public static void PlayMove(GameObject fromObject, (int x, int y) toCoordinates, GameObject toObject, Chess.EnumMoves howMove, int captures)
		{
			PlayerControl fromController = fromObject.GetComponent<PlayerControl>();
			PlayerControl toController = toObject.GetComponent<PlayerControl>();
			int nameIndexFrom = fromObject.name.IndexOf(':');
			int nameIndexTo = toObject.name.IndexOf(':');
			int team = char.IsUpper(fromObject.name[nameIndexFrom - 1]) ? 1 : 0;
			if (howMove == Chess.EnumMoves.Promotion)
			{
				int pieceIndex = 4;
				fromController.SetHat(Utils.PieceHats[pieceIndex], fromController.scannerCount);
				fromController.SetSkin(Utils.PieceSkins[pieceIndex]);
				fromController.SetName((team == 1 ? "Q" : "q") + fromObject.name[nameIndexFrom..]);
				nameIndexFrom = fromController.name.IndexOf(':');
			}
			switch (howMove)
			{
				case Chess.EnumMoves.KingCastle:
				case Chess.EnumMoves.QueenCastle:
					Chess.EnumMoves queenCastle = Chess.EnumMoves.QueenCastle;
					fromController.SetName(fromObject.name[nameIndexFrom - 1] + ":" + (howMove == queenCastle ? "2" : "6") + "," + toCoordinates.y.ToString());
					toController.SetName(toObject.name[nameIndexTo - 1] + ":" + (howMove == queenCastle ? "3" : "5") + "," + toCoordinates.y.ToString());
					fromObject.transform.position = new Vector3(howMove == queenCastle ? 17f : 19f, (toCoordinates.y * -0.5f) - 10f, fromObject.transform.position.z);
					toObject.transform.position = new Vector3(howMove == queenCastle ? 17.5f : 18.5f, (toCoordinates.y * -0.5f) - 10f, toObject.transform.position.z);
					break;
				case Chess.EnumMoves.EnPassant:
					fromController.SetName(fromObject.name[nameIndexFrom - 1] + ":" + toCoordinates.x.ToString() + "," + toCoordinates.y.ToString());
					toController.SetName(toObject.name[nameIndexTo - 1] + ":D");
					fromObject.transform.position = new Vector3((toCoordinates.x * 0.5f) + 16, (toCoordinates.y * -0.5f) - 10f, fromObject.transform.position.z);
					toObject.transform.position = new Vector3(25f + (captures % 10 * 0.5f), (float)((team == 1 ? -12f : -14.5f) + (Math.Floor(captures / 10f) * (team == 1 ? -0.5f : 0.5f))), toObject.transform.position.z);
					break;
				default:
					fromController.SetName(fromObject.name[nameIndexFrom - 1] + ":" + toCoordinates.x.ToString() + "," + toCoordinates.y.ToString());
					toController.SetName(toObject.name[nameIndexTo - 1] + ":D");
					fromObject.transform.position = new Vector3((toCoordinates.x * 0.5f) + 16, (toCoordinates.y * -0.5f) - 10f, fromObject.transform.position.z);
					toObject.transform.position = new Vector3(25f + (captures % 10 * 0.5f), (float)((team == 1 ? -12f : -14.5f) + (Math.Floor(captures / 10f) * (team == 1 ? -0.5f : 0.5f))), toObject.transform.position.z);
					break;
			}
		}

		public static void PlayMove(GameObject fromObject, (int x, int y) toCoordinates, Chess.EnumMoves howMove)
		{
			PlayerControl fromController = fromObject.GetComponent<PlayerControl>();
			int nameIndex = fromController.name.IndexOf(':');
			if (howMove == Chess.EnumMoves.Promotion)
			{
				int pieceIndex = 4;
				fromController.SetHat(Utils.PieceHats[pieceIndex], fromController.scannerCount);
				fromController.SetSkin(Utils.PieceSkins[pieceIndex]);
				fromController.SetName((char.IsUpper(fromObject.name[nameIndex - 1]) ? "Q" : "q") + fromObject.name[nameIndex..]);
				nameIndex = fromController.name.IndexOf(':');
			}
			fromController.SetName(fromObject.name[nameIndex - 1] + ":" + toCoordinates.x.ToString() + "," + toCoordinates.y.ToString());
			fromObject.transform.position = new Vector3((toCoordinates.x * 0.5f) + 16, (toCoordinates.y * -0.5f) - 10, fromObject.transform.position.z);
		}

		public static void GetAndPlayMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			GameObject fromObject = null;
			char[,] chessBoard = Chess.Chess.ChessBoard;
			Chess.EnumMoves howMove = Chess.EnumMoves.Normal;
			int directionY = char.IsUpper(chessBoard[fromCoordinates.y, fromCoordinates.x]) ? -1 : 1;
			float halfRank = chessBoard.GetLength(0) * 0.5f;
			if (Chess.Utils.ReadablePiece(chessBoard[fromCoordinates.y, fromCoordinates.x]) == 'P' && toCoordinates.y == halfRank + (halfRank * directionY)) howMove = Chess.EnumMoves.Promotion;
			Transform piecesObject = GameObject.Find("PiecesPath").transform;
			for (int i = 0; i < piecesObject.childCount; i++)
			{
				Transform elementObject = piecesObject.GetChild(i);
				int pieceNameIndex1 = elementObject.name.IndexOf(':');
				int pieceNameIndex2 = elementObject.name.IndexOf(',');
				if (pieceNameIndex1 == -1 || pieceNameIndex2 == -1) continue;
				(int x, int y) elementCoordinates = (int.Parse(elementObject.name[(pieceNameIndex1 + 1)..pieceNameIndex2]), int.Parse(elementObject.name[(pieceNameIndex2 + 1)..]));
				if (fromCoordinates.x == elementCoordinates.x && fromCoordinates.y == elementCoordinates.y)
				{
					fromObject = elementObject.gameObject.GetComponent<PlayerControl>().gameObject;
					break;
				}
			}
			if (chessBoard[toCoordinates.y, toCoordinates.x] != '1')
			{
				howMove = Chess.Utils.GetHowMove(fromCoordinates, toCoordinates);
				GameObject toObject = PlayerControl.LocalPlayer.gameObject;
				for (int i = 0; i < piecesObject.childCount; i++)
				{
					Transform elementObject = piecesObject.GetChild(i);
					int pieceNameIndex = elementObject.name.IndexOf(':');
					if (elementObject.name[(pieceNameIndex + 1)..] == toCoordinates.x + "," + toCoordinates.y)
					{
						toObject = elementObject.gameObject;
						break;
					}
				}
				PlayMove(fromObject, toCoordinates, toObject, howMove, char.IsUpper(chessBoard[fromCoordinates.y, fromCoordinates.x]) ? Chess.Chess.numCaptures.black++ : Chess.Chess.numCaptures.white++);
			}
			else
			{
				PlayMove(fromObject, toCoordinates, howMove);
			}
		}
	}
}
