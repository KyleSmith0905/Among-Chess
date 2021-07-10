using HarmonyLib;
using UnityEngine;
using Hazel;
using System;

namespace AmongChess.Source.Game
{
	internal class RealTime
	{
		private static float LocalTime = float.MaxValue;
		private static float TotalTime = float.MaxValue;

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
		public static class ShipStatusPatch
		{
			public static void Postfix()
			{
				if (Chess.Chess.Variant != "Real-Time") return;
				TotalTime = float.Parse(Chess.Chess.MainTime) * 60f;
			}
		}

		[HarmonyPatch(typeof(Utils), nameof(Utils.IncrementTurn))]
		private static class IncrementTurnPatch
		{
			public static bool Prefix()
			{
				if (Chess.Chess.Variant != "Real-Time") return true;
				Game.TotalTurns++;
				if (Game.TotalTurns < Game.AllPlayers.Count)
				{
					Game.PlayerTurn++;
					Game.LocalActivity = Game.AllPlayers[Game.PlayerTurn].PlayerId == PlayerControl.LocalPlayer.PlayerId ? EnumActivity.GameSelect : EnumActivity.GameWaiting;
					return false;
				}
				if (Game.LocalActivity == EnumActivity.Lobby) Game.LocalActivity = EnumActivity.GameWaiting;
				return false;
			}
		}

		[HarmonyPatch(typeof(Utils), nameof(Utils.AddIncrementTime))]
		private static class AddIncrementTimePatch
		{
			public static bool Prefix()
			{
				return Chess.Chess.Variant != "Real-Time";
			}
		}

		[HarmonyPatch(typeof(Utils), nameof(Utils.SynchronizeTime))]
		private static class SynchronizeTimePatch
		{
			public static bool Prefix()
			{
				return Chess.Chess.Variant != "Real-Time";
			}
		}

		[HarmonyPatch(typeof(Utils), nameof(Utils.SendCoordinates))]
		private static class SendCoordinatesPatch
		{
			public static void Prefix()
			{
				if (Chess.Chess.Variant != "Real-Time") return;
				LocalTime = float.Parse(Chess.Chess.IncrementTime);
			}
		}

		[HarmonyPatch(typeof(Clock), nameof(Clock.TimeManagement))]
		private static class TimeManagementPatch
		{
			public static bool Prefix()
			{
				if (Chess.Chess.Variant != "Real-Time") return true;
				if (Game.TotalTurns < Game.AllPlayers.Count) return false;
				CustomPlayer localPlayer = Utils.FindCustom(PlayerControl.LocalPlayer.PlayerId);
				if (LocalTime > 0f)
				{
					LocalTime -= Time.fixedDeltaTime;
					localPlayer.Activity = EnumActivity.GameWaiting;
				}
				else if (Game.LocalActivity == EnumActivity.GameWaiting)
				{
					localPlayer.Activity = EnumActivity.GameSelect;
				}
				TotalTime -= Time.fixedDeltaTime;
				if (0f >= TotalTime)
				{
					TotalTime = 0f;
					Game.EventEnded(Chess.EnumResults.DrawTimeout, PlayerControl.LocalPlayer.PlayerId, true);
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(Rpc.Rpc.HandleRpcPatch), nameof(Rpc.Rpc.HandleRpcPatch.Postfix))]
		private static class HandleRpcPatch
		{
			public static void Postfix(byte callId, MessageReader reader)
			{
				if (Chess.Chess.Variant != "Real-Time") return;
				if ((Rpc.EnumRpc)callId == Rpc.EnumRpc.MovePiece)
				{
					(byte x, byte y) fromCoordinates = (0, 0);
					fromCoordinates.x = reader.ReadByte();
					fromCoordinates.y = reader.ReadByte();
					Transform piecesObject = GameObject.Find("PiecesPath").transform;
					for (int i = 0; i < piecesObject.childCount; i++)
					{
						Transform elementObject = piecesObject.GetChild(i);
						int pieceNameIndex1 = elementObject.name.IndexOf(':');
						int pieceNameIndex2 = elementObject.name.IndexOf(',');
						if (pieceNameIndex1 == -1 || pieceNameIndex2 == -1) continue;
						(int x, int y) elementCoordinates = (int.Parse(elementObject.name[(pieceNameIndex1 + 1)..pieceNameIndex2]), int.Parse(elementObject.name[(pieceNameIndex2 + 1)..]));
						if (elementObject.name[0] == 't' && fromCoordinates.x == elementCoordinates.x && fromCoordinates.y == elementCoordinates.y)
						{
							int playerIndex = Utils.FindIndexById(PlayerControl.LocalPlayer.PlayerId);
							Utils.RevertMove(playerIndex, elementObject.gameObject.GetComponent<PlayerControl>());
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(SidePanel), nameof(SidePanel.AddTimeCounter))]
		private static class SidePanelPatch
		{
			public static bool Prefix([HarmonyArgument(0)] ref string results)
			{
				if (Chess.Chess.Variant != "Real-Time") return true;
				else if (TotalTime > 86400f) return false;
				TimeSpan time = TimeSpan.FromSeconds(TotalTime);
				string format = time.TotalHours >= 1 ? time.ToString(@"hh\:mm") : (time.TotalMinutes >= 1 ? time.ToString(@"mm\:ss") : time.ToString(@"ss\:ff"));
				results += SidePanel.GameOptionHudLayer2("Total Time", format);
				return false;
			}
		}
	}
}
