using HarmonyLib;
using TMPro;
using UnityEngine;

namespace AmongChess.Patches.GameControl
{
	public class GameEnd
	{
		public static int WinnerId = -1;

		[HarmonyPatch(typeof(EndGameManager))]
		public static class EndGameManagerPatch
		{
			[HarmonyPatch(nameof(EndGameManager.Start))]
			[HarmonyPrefix]
			public static void StartPatch(EndGameManager __instance)
			{
				int[] colorIds = (int[])Control.ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
				TextMeshPro winText = UnityEngine.Object.Instantiate(__instance.WinText);
				__instance.WinText.enabled = false;
				TempData.winners.Clear();
				if (WinnerId == -1)
				{
					winText.text = "Draw";
					winText.color = new Color32(255, 255, 255, 0);
				}
				else
				{
					int index = Utilities.FindIndexById((byte)WinnerId);
					winText.text = Control.ColorNames[colorIds[index]] + " won";
					winText.color = Palette.PlayerColors[colorIds[index]];
					TempData.winners.Add(new WinningPlayerData(Control.AllPlayers[index].Data));
				}

				Control.AllPlayers.Clear();
				Control.AllCustomPlayers.Clear();
			}
		}

		[HarmonyPatch(typeof(ShipStatus))]
		public static class ShipStatusPatch
		{
			[HarmonyPatch(nameof(ShipStatus.RpcEndGame))]
			[HarmonyPrefix]
			public static bool RpcEndGamePatch(GameOverReason endReason)
			{
				return endReason != GameOverReason.HumansByVote;
			}
		}
	}
}
