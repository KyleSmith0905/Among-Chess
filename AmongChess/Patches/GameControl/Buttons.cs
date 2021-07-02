using HarmonyLib;
using Hazel;
using UnityEngine;

namespace AmongChess.Patches.GameControl
{
	internal class Buttons
	{
		[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
		public static class HudManagerUpdatePatch
		{
			public static void Postfix(HudManager __instance)
			{
				__instance.MapButton.gameObject.active = false;
			}
		}

		[HarmonyPatch(typeof(ReportButtonManager))]
		public static class ReportButtonManagerPatch
		{
			[HarmonyPatch(nameof(ReportButtonManager.SetActive))]
			[HarmonyPrefix]
			public static bool SetActivePatch(ReportButtonManager __instance)
			{
				__instance.renderer.enabled = false;
				return false;
			}
		}

		[HarmonyPatch(typeof(UseButtonManager))]
		public static class UseButtonManagerPatch
		{
			public static int num = 0;

			[HarmonyPatch(nameof(UseButtonManager.SetTarget))]
			[HarmonyPrefix]
			public static bool SetTarget(UseButtonManager __instance)
			{
				if (Control.LocalActivity == EnumActivity.Lobby)
				{
					return true;
				}
				else if (Control.LocalActivity == EnumActivity.GameEnd)
				{
					__instance.UseButton.color = new Color(1f, 1f, 1f, 1f);
					return false;
				}
				num++;
				if (num > 3)
				{
					num = 0;
					GameObject piecesObjects = GameObject.Find("PiecesPath");
					if (piecesObjects == null) return true;
					ClearAllHighlighted();
					Color enabledColor = new Color(1f, 1f, 1f, 1f);
					Color disabledColor = new Color(1f, 1f, 1f, 0.3f);
					if (Control.LocalActivity == EnumActivity.GameSelect)
					{
						int[] colorIds = (int[])Control.ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
						int colorId = 0;
						for (int i = 0; i < colorIds.Length; i++) if (colorIds[i] == PlayerControl.LocalPlayer.Data.ColorId) { colorId = i; break; };
						PlayerControl target = Utilities.ClosestPiece(PlayerControl.LocalPlayer, colorId, out float distance);
						if (distance < 1)
						{
							__instance.UseButton.color = enabledColor;
							SpriteRenderer renderer = target.gameObject.transform.FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>();
							renderer.GetMaterial().SetFloat("_Outline", 1f);
							renderer.GetMaterial().SetColor("_OutlineColor", Color.yellow);
						}
						else
						{
							__instance.UseButton.color = disabledColor;
						}
					}
					else if (Control.LocalActivity == EnumActivity.GamePlace)
					{
						Vent target = Utilities.ClosestVent(PlayerControl.LocalPlayer, out float distance);
						if (distance < 1)
						{
							__instance.UseButton.color = enabledColor;
							SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
							renderer.GetMaterial().SetFloat("_Outline", 1);
							renderer.GetMaterial().SetColor("_OutlineColor", Color.yellow);
						}
						else
						{
							__instance.UseButton.color = disabledColor;
						}
					}
					else
					{
						__instance.UseButton.color = disabledColor;
					}
				}
				return false;
			}

			[HarmonyPatch(nameof(UseButtonManager.DoClick))]
			[HarmonyPrefix]
			public static bool DoClick()
			{
				if (Control.LocalActivity == EnumActivity.Lobby)
				{
					return true;
				}
				else if (Control.LocalActivity == EnumActivity.GamePlace)
				{
					Vent target = Utilities.ClosestVent(PlayerControl.LocalPlayer, out float distance);
					PlayerControl oldPlayer = PlayerControl.LocalPlayer;
					PlayerControl localPlayer = PlayerControl.LocalPlayer;
					Transform piecesObject = GameObject.Find("PiecesPath").transform;
					for (int i = 0; i < piecesObject.childCount; i++)
					{
						Transform elementObject = piecesObject.GetChild(i);
						if (elementObject.name[0] == 't')
						{
							oldPlayer = elementObject.gameObject.GetComponent<PlayerControl>();
							break;
						}
					}
					int targetNameIndex = target.name.IndexOf(',');
					int pieceNameIndex1 = oldPlayer.name.IndexOf(':');
					int pieceNameIndex2 = oldPlayer.name.IndexOf(',');
					(int x, int y) targetCoordinates = (x: int.Parse(target.name[..targetNameIndex]), y: int.Parse(target.name[(targetNameIndex + 1)..]));
					(int x, int y) pieceCoordinates = (x: int.Parse(oldPlayer.name[(pieceNameIndex1 + 1)..pieceNameIndex2]), y: int.Parse(oldPlayer.name[(pieceNameIndex2 + 1)..]));
					int playerIndex = Utilities.FindIndexById(PlayerControl.LocalPlayer.PlayerId);
					CustomPlayer customPlayer = Control.AllCustomPlayers[playerIndex];
					if (targetCoordinates.x == pieceCoordinates.x && targetCoordinates.y == pieceCoordinates.y)
					{
						Utilities.RevertMove(playerIndex, oldPlayer);
						return false;
					}
					ChessControl.EnumResults move = ChessControl.Control.MovePiece(pieceCoordinates, targetCoordinates, oldPlayer.gameObject);
					if (move == ChessControl.EnumResults.ErrorInvalid) return false;
					Utilities.RevertClothing(playerIndex);
					if (Control.TotalTurns % 10 == 0 && Control.TotalTurns > 0) Utilities.SynchronizeTime(customPlayer.Timer);
					Utilities.SendCoordinates(pieceCoordinates, targetCoordinates);
					MessageWriter rpcMessageReturn = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 66, (SendOption)1);
					rpcMessageReturn.Write(localPlayer.PlayerId);
					rpcMessageReturn.EndMessage();
					target.GetComponent<SpriteRenderer>().GetMaterial().SetFloat("_Outline", 0);
					oldPlayer.gameObject.active = true;
					Utilities.AddIncrementTime(playerIndex);
					Control.LocalActivity = EnumActivity.GameWaiting;
					Utilities.IncrementTurn();
					if ((int)move < 32)
					{
						Control.EventEnded(move, PlayerControl.LocalPlayer.PlayerId, true);
						return false;
					}
				}
				else if (Control.LocalActivity == EnumActivity.GameSelect)
				{
					int[] colorIds = (int[])Control.ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
					int colorId = 0;
					for (int i = 0; i < colorIds.Length; i++) if (colorIds[i] == PlayerControl.LocalPlayer.Data.ColorId) { colorId = i; break; };
					PlayerControl targetPlayer = Utilities.ClosestPiece(PlayerControl.LocalPlayer, colorId, out float distance);
					if (distance > 1) return false;
					PlayerControl localPlayer = PlayerControl.LocalPlayer;
					targetPlayer.transform.FindChild("Sprite").GetComponent<SpriteRenderer>().GetMaterial().SetFloat("_Outline", 0);
					int pieceIndex = Utilities.PieceIndex(targetPlayer.name[0]);
					targetPlayer.gameObject.active = false;
					localPlayer.transform.position = targetPlayer.transform.position;
					localPlayer.SetHat(Utilities.PieceHats[pieceIndex], localPlayer.Data.ColorId);
					localPlayer.SetSkin(Utilities.PieceSkins[pieceIndex]);
					localPlayer.SetPet(0u);
					MessageWriter rpcMessage = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 65, (SendOption)1);
					rpcMessage.Write(localPlayer.PlayerId);
					rpcMessage.Write((byte)pieceIndex);
					rpcMessage.EndMessage();
					targetPlayer.name = "t" + targetPlayer.name;
					Control.LocalActivity = EnumActivity.GamePlace;
				}
				else if (Control.LocalActivity == EnumActivity.GameEnd)
				{
					if (AmongUsClient.Instance.AmHost)
					{
						ShipStatus.RpcEndGame(GameOverReason.ImpostorByVote, false);
					}
					else
					{
						MessageWriter rpcMessage = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 69, (SendOption)1);
						rpcMessage.EndMessage();
					}
				}
				else if (Control.LocalActivity == EnumActivity.GameWaiting)
				{
					return false;
				}
				return false;
			}
		}

		public static void ClearAllHighlighted()
		{
			GameObject piecesObjects = GameObject.Find("PiecesPath");
			for (int i = 0; i < piecesObjects.transform.childCount; i++)
			{
				GameObject elementObject = piecesObjects.transform.GetChild(i).FindChild("Sprite").gameObject;
				SpriteRenderer renderer = elementObject.GetComponent<SpriteRenderer>();
				renderer.GetMaterial().SetFloat("_Outline", 0f);
			}
			GameObject ventObjects = GameObject.Find("VentPath");
			for (int i = 0; i < ventObjects.transform.childCount; i++)
			{
				GameObject elementObject = ventObjects.transform.GetChild(i).gameObject;
				SpriteRenderer renderer = elementObject.GetComponent<SpriteRenderer>();
				renderer.GetMaterial().SetFloat("_Outline", 0f);
			}
		}
	}
}
