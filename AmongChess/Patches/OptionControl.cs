using HarmonyLib;
using Hazel;
using Il2CppSystem.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AmongChess.Patches
{
	class OptionControl
	{
		public static List<OptionSingle> AllOption = new List<OptionSingle>();
		public static List<OptionGroupSingle> AllOptionGroup = new List<OptionGroupSingle>();

		public static List<OptionSingle> OptionDefault()
		{
			OptionSingle gameMode = new OptionSingle()
			{
				Id = 0,
				Name = "Game Mode",
				AllValues = new string[] { "Chess" }//, "Dev-Chess"}
			};
			OptionSingle variant = new OptionSingle()
			{
				Id = 1,
				Name = "Variant",
				AllValues = new string[] { "Normal" }
			};
			OptionSingle board = new OptionSingle()
			{
				Id = 2,
				Name = "Board",
				AllValues = new string[] { "Default", "Chess960" }
			};
			OptionSingle mainTime = new OptionSingle()
			{
				Id = 3,
				Name = "Main Time",
				AllValues = new string[] { "Unlimited", "0.5", "1", "2", "3", "5", "10", "30", "60" },
				GroupId = 0
			};
			OptionSingle incrementalTime = new OptionSingle()
			{
				Id = 4,
				Name = "Increment Time",
				AllValues = new string[] { "0", "0.5", "1", "2", "5", "10", "30" },
				GroupId = 0,
				Exemption = (3, new byte[] { 0 })
			};
			//if (AmongChess.dev) gameMode.AllValues.add  // Change to List<> first
			return new List<OptionSingle> { gameMode, variant, board, mainTime, incrementalTime }; ;
		}

		public static List<OptionGroupSingle> OptionGroupDefault()
		{
			List<OptionGroupSingle> allOptionGroup = new List<OptionGroupSingle> { };
			OptionGroupSingle timeControl = new OptionGroupSingle()
			{
				Id = 0,
				Name = "Time Control",
				Value = "{3} + {4}",
				Exemption = (3, new byte[] { 0 }, "Correspondence")
			};
			allOptionGroup.Add(timeControl);
			return allOptionGroup;
		}

		public static void GameOptionsExport()
		{
			StringBuilder stringSettings = new StringBuilder();
			for (int i = 0; i < AllOption.Count; i++)
			{
				_ = stringSettings.Append(AllOption[i].Id.ToString());
				_ = stringSettings.Append(",");
				_ = stringSettings.Append(AllOption[i].Value.ToString());
				_ = stringSettings.Append(";");
			}
			string path = Path.Combine(Application.persistentDataPath, "AmongChess-HostSettings");
			File.WriteAllText(path, stringSettings.ToString());
		}

		public static List<OptionSingle> GameOptionsImport()
		{
			List<OptionSingle> AllOptionsTemp = new List<OptionSingle>();
			List<OptionSingle> AllOptionsDefault = OptionDefault();
			try
			{
				string path = Path.Combine(Application.persistentDataPath, "AmongChess-HostSettings");
				string text = File.ReadAllText(path);
				List<string> splitText = text.Split(";").ToList();
				List<byte> collectedId = new List<byte> { };
				for (int i = 0; i < splitText.Count; i++)
				{
					int splitter = splitText[i].IndexOf(",");
					if (splitter == -1) break;
					byte optionId = byte.Parse(splitText[i][0..splitter]);
					byte valueId = byte.Parse(splitText[i][(splitter + 1)..]);
					OptionSingle optionSingle = AllOptionsDefault.Find(ele => ele.Id == optionId);
					if (optionSingle != null)
					{
						collectedId.Add(optionId);
						optionSingle.Id = optionId;
						optionSingle.Value = valueId;
						AllOptionsTemp.Add(optionSingle);
					}
				}
				for (int i = 0; i < AllOptionsDefault.Count; i++)
				{
					OptionSingle optionSingle = AllOptionsDefault[i];
					if (!collectedId.Contains(optionSingle.Id)) AllOptionsTemp.Add(optionSingle);
				}
			}
			catch
			{
				AllOptionsTemp = AllOptionsDefault;
			}
			return AllOptionsTemp;
		}

		[HarmonyPatch(typeof(GameOptionsMenu))]
		public static class GameOptionsMenuPatch
		{
			static readonly OptionSingle mainTimeOption = AllOption.Find(ele => ele.Id == 3);

			[HarmonyPrefix]
			[HarmonyPatch(nameof(GameOptionsMenu.Start))]
			public static bool GameOptionsMenuStart()
			{
				GameObject parentObject = GameObject.Find("SliderInner");
				StringOption stringOption = UnityEngine.Object.FindObjectOfType<StringOption>();
				parentObject.transform.position = new Vector3(parentObject.transform.position.x, 2.4f, parentObject.transform.position.z);
				for (int i = 0; i < AllOption.Count; i++)
				{
					StringOption option = UnityEngine.Object.Instantiate(stringOption, parentObject.transform);
					option.transform.position = new Vector3(option.transform.position.x, 3.35f - (i * 0.5f), option.transform.position.z);
					option.name = "Custom" + AllOption[i].Id;
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
					OptionSingle optionSingle = AllOption.Find(ele => "Custom" + ele.Id == childObject.name);
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
				OptionSingle option = AllOption.Find(ele => "Custom" + ele.Id == __instance.name);
				if (option.AllValues.Length - 1 > option.Value)
				{
					__instance.Value++;
					option.Value++;
					RpcPushDeltaOptions(option.Id, option.Value);
				}
				return false;
			}

			[HarmonyPatch(nameof(StringOption.Decrease))]
			[HarmonyPrefix]
			public static bool Decrease(StringOption __instance)
			{
				OptionSingle option = AllOption.Find(ele => "Custom" + ele.Id == __instance.name);
				if (0 < option.Value)
				{
					__instance.Value--;
					option.Value--;
					RpcPushDeltaOptions(option.Id, option.Value);
				}
				return false;
			}
		}

		[HarmonyPatch(typeof(SaveManager))]
		public static class SaveManagerLoadGameOptionsPatch
		{
			[HarmonyPatch(nameof(SaveManager.LoadGameOptions))]
			[HarmonyPrefix]
			public static void LoadGameOptionsPatch()
			{
				AllOptionGroup = OptionGroupDefault();
				AllOption = GameOptionsImport();
			}
		}

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
				for (int i = 0; i < AllOption.Count; i++)
				{
					OptionSingle optionSingle = AllOption.Find(ele => ele.Id == Ids[i]);
					if (!optionSingle.Exemption.Equals(default))
					{
						OptionSingle exemptionOption = AllOption.Find(ele => ele.Id == optionSingle.Exemption.Id);
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
						OptionGroupSingle optionGroupSingle = AllOptionGroup.Find(ele => ele.Id == optionSingle.GroupId);
						string groupValue = optionGroupSingle.Value;
						if (!optionGroupSingle.Exemption.Equals(default))
						{
							byte[] validValues = optionGroupSingle.Exemption.OptionValue;
							OptionSingle exemptionOption = AllOption.Find(ele => ele.Id == optionGroupSingle.Exemption.OptionId);
							if (Array.Exists(optionGroupSingle.Exemption.OptionValue, ele => ele == exemptionOption.Value)) groupValue = optionGroupSingle.Exemption.ValueTo;
						}
						int numVariables = groupValue.Count(ele => ele == '{');
						for (int j = 0; j < numVariables; j++)
						{
							int startIndex = groupValue.IndexOf('{');
							int endIndex = groupValue.IndexOf('}');
							byte optionId = byte.Parse(groupValue[(startIndex + 1)..endIndex]);
							OptionSingle variableOption = AllOption.Find(ele => ele.Id == optionId);
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

		[HarmonyPatch(typeof(GameStartManager))]
		public static class GameStartManagerPatch
		{
			[HarmonyPatch(nameof(GameStartManager.Start))]
			[HarmonyPrefix]
			public static void StartPatch(ref GameStartManager __instance)
			{
				GameEvents.PlayerActivity = 's';
				PlayerControl.GameOptions.MaxPlayers = 2;
				__instance.MinPlayers = 2;
				PlayerControl.GameOptions.MapId = 2;
				PlayerControl.GameOptions.CrewLightMod = 5;
			}

			[HarmonyPatch(nameof(GameStartManager.BeginGame))]
			[HarmonyPrefix]
			public static bool BeginGamePatch(GameStartManager __instance)
			{
				OptionSingle GameMode = AllOption.Find(ele => ele.Name == "Game Mode");
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
			[HarmonyPatch(nameof(PlayerControl.RpcSyncSettings))]
			[HarmonyPostfix]
			public static void RpcSyncSettingsPatch()
			{
				if (PlayerControl.AllPlayerControls.Count > 1 && AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
				{
					MessageWriter rpcMessage = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 68, (SendOption)1);
					for (int i = 0; i < AllOption.Count; i++)
					{
						rpcMessage.Write(AllOption[i].Id);
						rpcMessage.Write((byte)AllOption[i].Value);
					}
					rpcMessage.EndMessage();
				}
			}

			[HarmonyPatch(nameof(PlayerControl.RpcSetInfected))]
			[HarmonyPostfix]
			public static void RpcSetInfected()
			{
				if (AmongUsClient.Instance.AmHost)
				{
					GameEvents.AllPlayers = new List<PlayerControl> { };
					int playersCount = GameData.Instance.PlayerCount;
					int[] colorsArray = (int[])GameEvents.ColorIds.GetValue(playersCount - 1);
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

		public static void RpcPushDeltaOptions(byte optionId, uint optionValue)
		{
			GameOptionsExport();
			MessageWriter rpcMessage = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 68, (SendOption)1);
			rpcMessage.Write(optionId);
			rpcMessage.Write((byte)optionValue);
			rpcMessage.EndMessage();
		}
	}
}