using HarmonyLib;
using UnityEngine;

namespace AmongChess.Patches.LobbyControl
{
	internal class OptionMenu
	{
		[HarmonyPatch(typeof(GameOptionsMenu))]
		public static class GameOptionsMenuPatch
		{
			private static readonly OptionSingle mainTimeOption = OptionControl.AllOption.Find(ele => ele.Id == 3);

			[HarmonyPrefix]
			[HarmonyPatch(nameof(GameOptionsMenu.Start))]
			public static bool GameOptionsMenuStart()
			{
				GameObject parentObject = GameObject.Find("SliderInner");
				StringOption stringOption = UnityEngine.Object.FindObjectOfType<StringOption>();
				parentObject.transform.position = new Vector3(parentObject.transform.position.x, 2.4f, parentObject.transform.position.z);
				for (int i = 0; i < OptionControl.AllOption.Count; i++)
				{
					StringOption option = UnityEngine.Object.Instantiate(stringOption, parentObject.transform);
					option.transform.position = new Vector3(option.transform.position.x, 3.35f - (i * 0.5f), option.transform.position.z);
					option.name = "Custom" + OptionControl.AllOption[i].Id;
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
					OptionSingle optionSingle = OptionControl.AllOption.Find(ele => "Custom" + ele.Id == childObject.name);
					if (optionSingle == null) continue;
					string valueText = optionSingle.AllValues[optionSingle.Value];
					StringOption option = childObject.GetComponent<StringOption>();
					option.TitleText.text = optionSingle.Name;
					option.Value = optionSingle.Value;
					option.ValueText.text = optionSingle.AllValues[optionSingle.Value];
					switch (int.Parse(childObject.name[6..]))
					{
						case 4:
							childObject.active = mainTimeOption.Value != 0;
							break;
					}
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
				OptionSingle option = OptionControl.AllOption.Find(ele => "Custom" + ele.Id == __instance.name);
				if (option.AllValues.Length - 1 > option.Value)
				{
					__instance.Value++;
					option.Value++;
					OptionControl.RpcPushDeltaOptions(option.Id, option.Value);
				}
				return false;
			}

			[HarmonyPatch(nameof(StringOption.Decrease))]
			[HarmonyPrefix]
			public static bool Decrease(StringOption __instance)
			{
				OptionSingle option = OptionControl.AllOption.Find(ele => "Custom" + ele.Id == __instance.name);
				if (0 < option.Value)
				{
					__instance.Value--;
					option.Value--;
					OptionControl.RpcPushDeltaOptions(option.Id, option.Value);
				}
				return false;
			}
		}
	}
}
