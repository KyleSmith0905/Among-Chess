using HarmonyLib;
using Hazel;
using System.Collections.Generic;

namespace AmongChess.Patches.LobbyControl
{
	internal class OptionControl
	{
		public static List<OptionSingle> AllOption = new List<OptionSingle>();
		public static List<OptionGroupSingle> AllOptionGroup = new List<OptionGroupSingle>();

		[HarmonyPatch(typeof(SaveManager))]
		public static class SaveManagerLoadGameOptionsPatch
		{
			[HarmonyPatch(nameof(SaveManager.LoadGameOptions))]
			[HarmonyPrefix]
			public static void LoadGameOptionsPatch()
			{
				AllOptionGroup = OptionGroupDefault();
				AllOption = SaveControl.GameOptionsImport();
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

		public static List<OptionSingle> OptionDefault()
		{
			OptionSingle gameMode = new OptionSingle()
			{
				Id = 0,
				Name = "Game Mode",
				AllValues = new string[] { "Chess" }
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

		public static void RpcPushDeltaOptions(byte optionId, uint optionValue)
		{
			SaveControl.GameOptionsExport();
			MessageWriter rpcMessage = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 68, (SendOption)1);
			rpcMessage.Write(optionId);
			rpcMessage.Write((byte)optionValue);
			rpcMessage.EndMessage();
		}
	}
}
