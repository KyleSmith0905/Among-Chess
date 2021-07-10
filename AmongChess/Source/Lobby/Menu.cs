using HarmonyLib;
using UnityEngine;

namespace AmongChess.Source.Lobby
{
	internal class Menu
	{
		[HarmonyPatch(typeof(GameOptionsMenu))]
		public static class GameOptionsMenuPatch
		{
			private static readonly ClassOption mainTimeOption = Options.AllOption.Find(ele => ele.Id == 3);

			[HarmonyPrefix]
			[HarmonyPatch(nameof(GameOptionsMenu.Start))]
			public static bool GameOptionsMenuStart()
			{
				GameObject parentObject = GameObject.Find("SliderInner");
				StringOption stringOption = Object.FindObjectOfType<StringOption>();
				parentObject.transform.position = new Vector3(parentObject.transform.position.x, 2.4f, parentObject.transform.position.z);
				for (int i = 0; i < Options.AllOption.Count; i++)
				{
					StringOption option = Object.Instantiate(stringOption, parentObject.transform);
					option.transform.position = new Vector3(option.transform.position.x, 3.35f - (i * 0.5f), option.transform.position.z);
					option.name = "Custom" + Options.AllOption[i].Id;
					option.Values = new StringNames[] { 0 };
				}
				for (int i = 0; i < parentObject.transform.childCount; i++)
				{
					GameObject child = parentObject.transform.GetChild(i).gameObject;
					if (child && !child.name.StartsWith("Custom")) child.SetActive(false);
				}
				return true;
			}

			[HarmonyPrefix]
			[HarmonyPatch(nameof(GameOptionsMenu.Update))]
			public static bool GameOptionsMenuUpdate()
			{
				GameObject parentObject = GameObject.Find("SliderInner");
				for (int i = 0; i < parentObject.transform.childCount; i++)
				{
					GameObject childObject = parentObject.transform.GetChild(i).gameObject;
					ClassOption optionSingle = Options.AllOption.Find(ele => "Custom" + ele.Id == childObject.name);
					if (optionSingle == null) continue;
					string valueText = optionSingle.AllValues[optionSingle.Value];
					StringOption option = childObject.GetComponent<StringOption>();
					option.TitleText.text = optionSingle.Name;
					option.Value = optionSingle.Value;
					option.ValueText.text = optionSingle.AllValues[optionSingle.Value];
				}
				Scroller menuScroller = parentObject.transform.parent.GetComponent<Scroller>();
				menuScroller.YBounds.min = 0;
				menuScroller.YBounds.max = 0;
				return false;
			}
		}

		[HarmonyPatch(typeof(StringOption))]
		public static class StringOptionPatch
		{
			[HarmonyPatch(nameof(StringOption.Increase))]
			[HarmonyPrefix]
			public static bool Increase(StringOption __instance)
			{
				ClassOption option = Options.AllOption.Find(ele => "Custom" + ele.Id == __instance.name);
				if (option.AllValues.Length - 1 > option.Value)
				{
					__instance.Value++;
					option.Value++;
					Options.RpcPushDeltaOptions(option.Id, option.Value);
				}
				return false;
			}

			[HarmonyPatch(nameof(StringOption.Decrease))]
			[HarmonyPrefix]
			public static bool Decrease(StringOption __instance)
			{
				ClassOption option = Options.AllOption.Find(ele => "Custom" + ele.Id == __instance.name);
				if (0 < option.Value)
				{
					__instance.Value--;
					option.Value--;
					Options.RpcPushDeltaOptions(option.Id, option.Value);
				}
				return false;
			}
		}
	}
}
