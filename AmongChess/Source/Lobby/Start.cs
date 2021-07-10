using HarmonyLib;
using System.Collections.Generic;

namespace AmongChess.Source.Lobby
{
	internal class Start
	{
		[HarmonyPatch(typeof(GameStartManager))]
		public static class GameStartManagerPatch
		{
			[HarmonyPatch(nameof(GameStartManager.Start))]
			[HarmonyPrefix]
			public static void StartPatch(ref GameStartManager __instance)
			{
				Game.Game.AllCustomPlayers.Clear();
				Game.Game.AllPlayers.Clear();
				PlayerControl.GameOptions.MaxPlayers = 2;
				__instance.MinPlayers = 2;
				PlayerControl.GameOptions.MapId = 2;
				PlayerControl.GameOptions.CrewLightMod = 5f;
				PlayerControl.GameOptions.PlayerSpeedMod = 1f;
			}

			[HarmonyPatch(nameof(GameStartManager.BeginGame))]
			[HarmonyPrefix]
			public static bool BeginGamePatch(GameStartManager __instance)
			{
				ClassOption GameMode = Options.AllOption.Find(ele => ele.Name == "Game Mode");
				if (GameMode.AllValues[GameMode.Value] == "Dev-Chess") __instance.ReallyBegin(false);
				if (__instance.startState == GameStartManager.StartingStates.NotStarting)
				{
					if (GameData.Instance.PlayerCount < __instance.MinPlayers)
					{
						_ = __instance.StartCoroutine(Effects.SwayX(__instance.PlayerCounter.transform));
					}
					else
					{
						__instance.ReallyBegin(neverShow: false);
					}
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl))]
		private class PlayerControlPatch
		{
			[HarmonyPatch(nameof(PlayerControl.RpcSetInfected))]
			[HarmonyPostfix]
			public static void RpcSetInfected()
			{
				if (AmongUsClient.Instance.AmHost)
				{
					Game.Game.AllPlayers = new List<PlayerControl> { };
					int playersCount = GameData.Instance.PlayerCount;
					int[] colorsArray = (int[])Game.Game.ColorIds.GetValue(playersCount - 1);
					List<int> colorsList = new List<int> { };
					for (int i = 0; i < colorsArray.Length; i++) colorsList.Add(colorsArray[i]);
					for (int i = 0; i < playersCount; i++)
					{
						int random = UnityEngine.Random.RandomRangeInt(0, playersCount - i);
						PlayerControl playerControl = PlayerControl.AllPlayerControls[i];
						playerControl.RpcSetColor((byte)colorsList[random]);
						colorsList.RemoveAt(random);
					}
				}
			}
		}
	}
}
