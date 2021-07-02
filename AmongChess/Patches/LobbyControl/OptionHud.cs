using HarmonyLib;
using System;
using System.Linq;

namespace AmongChess.Patches.LobbyControl
{
	internal class OptionHud
	{
		[HarmonyPatch(typeof(GameOptionsData))]
		public static class GameOptionsDataPatch
		{
			private static string GameOptionHudLayer1(string Name, string Value)
			{
				return "<color=#FFFFFF>┃\n┣</color> <color=#BBBBFF>" + Name + ": " + Value + "\n</color>";
			}

			private static string GameOptionHudLayer2(string Name, string Value)
			{
				return "<color=#FFFFFF>┃</color> <color=#BBBBFF>┃</color>\n<color=#FFFFFF>┃</color> <color=#BBBBFF>┣</color> <color=#7777FF>" + Name + ": " + Value + "\n</color>";
			}

			[HarmonyPatch(nameof(GameOptionsData.ToHudString))]
			[HarmonyPrefix]
			public static bool ToHudString(ref string __result, ref GameOptionsData __instance)
			{
				__result = "<color=#FFFFFF>Among Chess Settings\n</color>";
				int[] Ids = { 0, 1, 2, 3, 4 };
				byte lastGroup = byte.MaxValue;
				for (int i = 0; i < OptionControl.AllOption.Count; i++)
				{
					OptionSingle optionSingle = OptionControl.AllOption.Find(ele => ele.Id == Ids[i]);
					if (!optionSingle.Exemption.Equals(default))
					{
						OptionSingle exemptionOption = OptionControl.AllOption.Find(ele => ele.Id == optionSingle.Exemption.Id);
						if (Array.Exists(optionSingle.Exemption.Values, ele => ele == exemptionOption.Value)) continue;
					}
					if (optionSingle.GroupId == byte.MaxValue)
					{
						lastGroup = byte.MaxValue;
						__result += GameOptionHudLayer1(optionSingle.Name, optionSingle.AllValues[optionSingle.Value]);
					}
					else
					{
						if (lastGroup == optionSingle.GroupId)
						{
							__result += GameOptionHudLayer2(optionSingle.Name, optionSingle.AllValues[optionSingle.Value]);
							continue;
						}
						OptionGroupSingle optionGroupSingle = OptionControl.AllOptionGroup.Find(ele => ele.Id == optionSingle.GroupId);
						string groupValue = optionGroupSingle.Value;
						if (!optionGroupSingle.Exemption.Equals(default))
						{
							byte[] validValues = optionGroupSingle.Exemption.OptionValue;
							OptionSingle exemptionOption = OptionControl.AllOption.Find(ele => ele.Id == optionGroupSingle.Exemption.OptionId);
							if (Array.Exists(optionGroupSingle.Exemption.OptionValue, ele => ele == exemptionOption.Value)) groupValue = optionGroupSingle.Exemption.ValueTo;
						}
						int numVariables = groupValue.Count(ele => ele == '{');
						for (int j = 0; j < numVariables; j++)
						{
							int startIndex = groupValue.IndexOf('{');
							int endIndex = groupValue.IndexOf('}');
							byte optionId = byte.Parse(groupValue[(startIndex + 1)..endIndex]);
							OptionSingle variableOption = OptionControl.AllOption.Find(ele => ele.Id == optionId);
							groupValue = groupValue.Remove(startIndex, endIndex - startIndex + 1);
							groupValue = groupValue.Insert(startIndex, variableOption.AllValues[variableOption.Value]);
						}
						lastGroup = optionSingle.GroupId;
						__result += GameOptionHudLayer1(optionGroupSingle.Name, groupValue);
						__result += GameOptionHudLayer2(optionSingle.Name, optionSingle.AllValues[optionSingle.Value]);
					}
				}
				__result = "<size=1.75>" + __result + "</size>";
				__result = "<line-height=1.65>" + __result + "</line-height>";
				return false;
			}
		}
	}
}
