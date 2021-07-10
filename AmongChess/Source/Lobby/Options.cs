using HarmonyLib;
using Hazel;
using System.Collections.Generic;

namespace AmongChess.Source.Lobby
{
	internal class Options
	{
		public static List<ClassOption> AllOption = new List<ClassOption>();
		public static List<ClassOptionGroup> AllOptionGroup = new List<ClassOptionGroup>();

		[HarmonyPatch(typeof(SaveManager))]
		public static class SaveManagerLoadGameOptionsPatch
		{
			[HarmonyPatch(nameof(SaveManager.LoadGameOptions))]
			[HarmonyPrefix]
			public static void LoadGameOptionsPatch()
			{
				AllOptionGroup = OptionGroupDefault();
				AllOption = Save.GameOptionsImport();
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
						rpcMessage.Write(AllOption[i].Value);
					}
					rpcMessage.EndMessage();
				}
			}
		}

		public static List<ClassOption> OptionDefault()
		{
			ClassOption gameMode = new ClassOption()
			{
				Id = 0,
				Name = "Game Mode",
				AllValues = new string[] { "Chess" }
			};
			ClassOption variant = new ClassOption()
			{
				Id = 1,
				Name = "Variant",
				AllValues = new string[] { "Normal", "Real-Time" }
			};
			ClassOption board = new ClassOption()
			{
				Id = 2,
				Name = "Board",
				AllValues = new string[] { "Default", "Chess960", "Transcendental"}
			};
			ClassOption mainTime = new ClassOption()
			{
				Id = 3,
				Name = "Main Time",
				AllValues = new string[] { "Unlimited", "0.5", "1", "2", "3", "5", "10", "30", "60" },
				GroupId = 0
			};
			ClassOption incrementalTime = new ClassOption()
			{
				Id = 4,
				Name = "Increment Time",
				AllValues = new string[] { "0", "0.5", "1", "2", "5", "10", "30" },
				GroupId = 0,
			};
			return new List<ClassOption> { gameMode, variant, board, mainTime, incrementalTime }; ;
		}

		public static List<ClassOptionGroup> OptionGroupDefault()
		{
			List<ClassOptionGroup> allOptionGroup = new List<ClassOptionGroup> { };
			ClassOptionGroup timeControl = new ClassOptionGroup()
			{
				Id = 0,
				Name = "Time Control",
				Value = "{3} + {4}",
			};
			allOptionGroup.Add(timeControl);
			return allOptionGroup;
		}

		public static void RpcPushDeltaOptions(byte optionId, uint optionValue)
		{
			Save.GameOptionsExport();
			MessageWriter rpcMessage = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 68, (SendOption)1);
			rpcMessage.Write(optionId);
			rpcMessage.Write((byte)optionValue);
			rpcMessage.EndMessage();
		}
	}
}
