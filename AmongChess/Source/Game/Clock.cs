using HarmonyLib;
using UnityEngine;

namespace AmongChess.Source.Game
{
	public class Clock
	{
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
		public static class ShipStatusPatch
		{
			public static void Prefix()
			{
				if (Game.LocalActivity == EnumActivity.GameEnd || Game.LocalActivity == EnumActivity.Lobby || Game.TotalTurns <= 0) return;
				TimeManagement();
			}
		}


		public static void TimeManagement()
		{
			Game.AllCustomPlayers[Game.PlayerTurn].Timer -= Time.fixedDeltaTime;
			if (0 >= Game.AllCustomPlayers[Game.PlayerTurn].Timer)
			{
				Game.AllCustomPlayers[Game.PlayerTurn].Timer = 0f;
				Game.EventEnded(Chess.EnumResults.WinTimeout, Game.PlayerTurn == 0 ? (byte)(Game.AllPlayers.Count - 1) : (byte)(Game.PlayerTurn - 1), true);
			}
		}
	}
}
