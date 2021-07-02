using HarmonyLib;
using UnityEngine;

namespace AmongChess.Patches.GameControl
{
	public class TimeManager
	{
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
		public static class ShipStatusPatch
		{
			public static void Prefix()
			{
				if (Control.LocalActivity == EnumActivity.GameEnd || Control.LocalActivity == EnumActivity.Lobby || Control.TotalTurns <= 0) return;
				TimeManagement();
			}
		}


		public static void TimeManagement()
		{
			Control.AllCustomPlayers[Control.PlayerTurn].Timer -= Time.fixedDeltaTime;
			if (0 >= Control.AllCustomPlayers[Control.PlayerTurn].Timer)
			{
				Control.AllCustomPlayers[Control.PlayerTurn].Timer = 0f;
				Control.EventEnded(ChessControl.EnumResults.WinTimeout, Control.PlayerTurn == 0 ? (byte)(Control.AllPlayers.Count - 1) : (byte)(Control.PlayerTurn - 1), true);
			}
		}
	}
}
