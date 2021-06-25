/*
 * [ ] Increment time out of sync
 * [ ] Every ~10 turns send RPC to resync time just in case.
 * [ ] Fix randomize who is white
 * [ ] Create Among Us hats for characters
 * [ ] Chess bot
 * [ ] Add maps
 * [ ] Check if square vents exist on Polus
 * [ ] Scholars mate does not result in checkmate
 */

using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;
using Reactor.Patches;
using UnityEngine;

namespace AmongChess
{
	[BepInPlugin("kylesmith0905.amongchess", "AmongChess", version)]
	[BepInProcess("Among Us.exe")]
	[BepInDependency(ReactorPlugin.Id)]
	[ReactorPluginSide(PluginSide.ClientOnly)]
	public class AmongChess : BasePlugin
	{
		public const string version = "1.0.0";

		public Harmony Harmony { get; } = new Harmony("kylesmith0905.amongchess");

		public override void Load()
		{
			ReactorVersionShower.TextUpdated += (text) =>
			{
				text.faceColor = new Color32(255, 165, 0, 255);
				text.fontSize = 3.2f;
				text.text = "Among Chess v" + version;
			};
			Harmony.PatchAll();
		}

		[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Show))]
		public static class CreateGameOptionsShowPatch
		{
			public static bool Prefix(CreateGameOptions __instance)
			{
				_ = __instance.StartCoroutine(__instance.CoStartGame());
				return false;
			}
		}
	}
}