using HarmonyLib;
using System;
using UnityEngine;

namespace AmongChess.Source.Game
{
	internal class SidePanel
	{
		[HarmonyPatch(typeof(TaskPanelBehaviour))]
		public static class TaskPanelBehaviourPatch
		{
			private static GameObject progressTracker = null;
			private static GameObject tab = null;
			private static GameObject background = null;

			[HarmonyPatch(nameof(TaskPanelBehaviour.Update))]
			[HarmonyPrefix]
			public static bool UpdatePatch(TaskPanelBehaviour __instance)
			{
				if (progressTracker == null) progressTracker = __instance.transform.parent.FindChild("ProgressTracker").gameObject;
				if (tab == null) tab = __instance.transform.FindChild("Tab").gameObject;
				if (background == null) background = __instance.transform.FindChild("Background").gameObject;
				progressTracker.active = false;
				tab.active = false;
				background.active = false;
				__instance.transform.localPosition = new Vector3(-5.25f, 3f, 0f);
				int playerCount = Game.AllPlayers.Count;
				if (playerCount < 1) return true;
				int[] colorIds = (int[])Game.ColorIds.GetValue(playerCount - 1);
				Lobby.ClassOption optionMainTime = Lobby.Options.AllOption.Find(ele => ele.Id == 3);
				Lobby.ClassOption optionIncrementTime = Lobby.Options.AllOption.Find(ele => ele.Id == 4);
				string timeControl = optionMainTime.AllValues[optionMainTime.Value] + " + " + optionIncrementTime.AllValues[optionIncrementTime.Value];
				string results = "Among Chess Details\n";
				results += GameOptionHudLayer1("Turn", Game.ColorNames[colorIds[Game.PlayerTurn]].ToString());
				results += GameOptionHudLayer1("Time", timeControl);
				AddTimeCounter(ref results);
				results += GameOptionHudLayer1("Total Moves", Game.TotalTurns.ToString());
				results = "<line-height=2>" + results + "</line-height>";
				results = "<size=2>" + results + "</size>";
				__instance.TaskText.text = results;
				return false;
			}

			[HarmonyPatch(nameof(TaskPanelBehaviour.ToggleOpen))]
			[HarmonyPrefix]
			public static bool ToggleOpenPatch(TaskPanelBehaviour __instance)
			{
				__instance.open = false;
				return false;
			}
		}

		public static string GameOptionHudLayer1(string Name, string Value)
		{
			return "<color=#FFFFFF>┃\n┣</color> <color=#BBBBFF>" + Name + ": " + Value + "\n</color>";
		}

		public static string GameOptionHudLayer2(string Name, string Value)
		{
			return "<color=#FFFFFF>┃</color> <color=#BBBBFF>┃</color>\n<color=#FFFFFF>┃</color> <color=#BBBBFF>┣</color> <color=#7777FF>" + Name + ": " + Value + "\n</color>";
		}

		public static void AddTimeCounter(ref string result)
		{
			int playerCount = Game.AllPlayers.Count;
			int[] colorIds = (int[])Game.ColorIds.GetValue(playerCount - 1);
			for (int i = 0; i < playerCount; i++)
			{
				if (Chess.Chess.MainTime == "Unlimited" || Game.AllCustomPlayers.Count != playerCount) break;
				float timer = Game.AllCustomPlayers[i].Timer;
				if (timer == float.MaxValue) break;
				TimeSpan time = TimeSpan.FromSeconds(timer);
				string format = time.TotalHours >= 1 ? time.ToString(@"hh\:mm") : (time.TotalMinutes >= 1 ? time.ToString(@"mm\:ss") : time.ToString(@"ss\:ff"));
				result += GameOptionHudLayer2(Game.ColorNames[colorIds[i]].ToString(), format);
			}
		}
	}
}
