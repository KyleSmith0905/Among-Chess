using HarmonyLib;
using Hazel;
using UnityEngine;

namespace AmongChess.Source.Game
{
	internal class Buttons
	{
		[HarmonyPatch(typeof(HudManager))]
		public static class HudManagerUpdatePatch
		{
			[HarmonyPatch(nameof(HudManager.Update))]
			[HarmonyPrefix]
			public static void Postfix(HudManager __instance)
			{
				__instance.MapButton.gameObject.active = false;
			}

			[HarmonyPatch(nameof(HudManager.ShowMap))]
			[HarmonyPrefix]
			public static bool Prefix()
			{
				return false;
			}
		}

		[HarmonyPatch(typeof(ReportButtonManager))]
		public static class ReportButtonManagerPatch
		{
			[HarmonyPatch(nameof(ReportButtonManager.SetActive))]
			[HarmonyPrefix]
			public static bool SetActivePatch(ReportButtonManager __instance)
			{
				__instance.gameObject.active = false;
				return false;
			}
		}

		[HarmonyPatch(typeof(UseButtonManager))]
		public static class UseButtonManagerPatch
		{
			public static int num = 0;

			public static void ActivateButton(UseButtonManager instance)
			{
				instance.currentButtonShown.text.color = new Color(1f, 1f, 1f, 1f);
				instance.currentButtonShown.graphic.color = new Color(1f, 1f, 1f, 1f);
			}

			public static void DeactivateButton(UseButtonManager instance)
			{
				instance.currentButtonShown.text.color = new Color(1f, 1f, 1f, 0.3f);
				instance.currentButtonShown.graphic.color = new Color(1f, 1f, 1f, 0.3f);
			}

			[HarmonyPatch(nameof(UseButtonManager.SetTarget))]
			[HarmonyPrefix]
			public static bool SetTarget(UseButtonManager __instance)
			{
				if (Game.LocalActivity == EnumActivity.Lobby)
				{
					return true;
				}
				else if (Game.LocalActivity == EnumActivity.GameEnd)
				{
					ActivateButton(__instance);
					return false;
				}
				num++;
				if (num > 3)
				{
					num = 0;
					GameObject piecesObjects = GameObject.Find("PiecesPath");
					if (piecesObjects == null) return true;
					ClearAllHighlighted();
					if (Game.LocalActivity == EnumActivity.GameSelect)
					{
						int[] colorIds = (int[])Game.ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
						int colorId = 0;
						for (int i = 0; i < colorIds.Length; i++) if (colorIds[i] == PlayerControl.LocalPlayer.Data.ColorId) { colorId = i; break; };
						PlayerControl target = Utils.ClosestPiece(PlayerControl.LocalPlayer, colorId, out float distance);
						if (distance < 1)
						{
							ActivateButton(__instance);
							SpriteRenderer renderer = target.gameObject.transform.FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>();
							renderer.GetMaterial().SetFloat("_Outline", 1f);
							renderer.GetMaterial().SetColor("_OutlineColor", Color.yellow);
						}
						else
						{
							DeactivateButton(__instance);
						}
					}
					else if (Game.LocalActivity == EnumActivity.GamePlace)
					{
						Vent target = Utils.ClosestVent(PlayerControl.LocalPlayer, out float distance);
						if (distance < 1)
						{
							ActivateButton(__instance);
							SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
							renderer.GetMaterial().SetFloat("_Outline", 1);
							renderer.GetMaterial().SetColor("_OutlineColor", Color.yellow);
						}
						else
						{
							DeactivateButton(__instance);
						}
					}
					else
					{
						DeactivateButton(__instance);
					}
				}
				return false;
			}

			[HarmonyPatch(nameof(UseButtonManager.DoClick))]
			[HarmonyPrefix]
			public static bool DoClick()
			{
				if (Game.LocalActivity == EnumActivity.Lobby)
				{
					return true;
				}
				else if (Game.LocalActivity == EnumActivity.GamePlace)
				{
					Vent target = Utils.ClosestVent(PlayerControl.LocalPlayer, out float distance);
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
					int playerIndex = Utils.FindIndexById(PlayerControl.LocalPlayer.PlayerId);
					CustomPlayer customPlayer = Game.AllCustomPlayers[playerIndex];
					if (targetCoordinates.x == pieceCoordinates.x && targetCoordinates.y == pieceCoordinates.y)
					{
						Utils.RevertMove(playerIndex, oldPlayer);
						return false;
					}
					Chess.EnumResults move = Chess.Chess.MovePiece(pieceCoordinates, targetCoordinates, oldPlayer.gameObject);
					if (move == Chess.EnumResults.ErrorInvalid) return false;
					Utils.RevertClothing(playerIndex);
					if (Game.TotalTurns % 10 == 0 && Game.TotalTurns > 0) Utils.SynchronizeTime(customPlayer.Timer);
					Utils.SendCoordinates(pieceCoordinates, targetCoordinates);
					MessageWriter rpcMessageReturn = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 66, (SendOption)1);
					rpcMessageReturn.Write(localPlayer.PlayerId);
					rpcMessageReturn.EndMessage();
					target.GetComponent<SpriteRenderer>().GetMaterial().SetFloat("_Outline", 0);
					oldPlayer.gameObject.active = true;
					Utils.AddIncrementTime(playerIndex);
					Game.LocalActivity = EnumActivity.GameWaiting;
					Utils.IncrementTurn();
					if ((int)move < 32)
					{
						Game.EventEnded(move, PlayerControl.LocalPlayer.PlayerId, true);
						return false;
					}
				}
				else if (Game.LocalActivity == EnumActivity.GameSelect)
				{
					int[] colorIds = (int[])Game.ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
					int colorId = 0;
					for (int i = 0; i < colorIds.Length; i++) if (colorIds[i] == PlayerControl.LocalPlayer.Data.ColorId) { colorId = i; break; };
					PlayerControl targetPlayer = Utils.ClosestPiece(PlayerControl.LocalPlayer, colorId, out float distance);
					if (distance > 1) return false;
					PlayerControl localPlayer = PlayerControl.LocalPlayer;
					targetPlayer.transform.FindChild("Sprite").GetComponent<SpriteRenderer>().GetMaterial().SetFloat("_Outline", 0);
					int pieceIndex = Utils.PieceIndex(targetPlayer.name[0]);
					targetPlayer.gameObject.active = false;
					localPlayer.transform.position = targetPlayer.transform.position;
					localPlayer.SetHat(Utils.PieceHats[pieceIndex], localPlayer.Data.ColorId);
					localPlayer.SetSkin(Utils.PieceSkins[pieceIndex]);
					localPlayer.SetPet(0u);
					MessageWriter rpcMessage = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 65, (SendOption)1);
					rpcMessage.Write(localPlayer.PlayerId);
					rpcMessage.Write((byte)pieceIndex);
					rpcMessage.EndMessage();
					targetPlayer.name = "t" + targetPlayer.name;
					Game.LocalActivity = EnumActivity.GamePlace;
				}
				else if (Game.LocalActivity == EnumActivity.GameEnd)
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
				else if (Game.LocalActivity == EnumActivity.GameWaiting)
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
