using HarmonyLib;
using Hazel;
using TMPro;
using UnityEngine;

namespace AmongChess.Source.Game
{
	internal class Start
	{
		[HarmonyPatch(typeof(IntroCutscene))]
		public static class IntroCutScenePatch
		{
			[HarmonyPatch(nameof(IntroCutscene.BeginCrewmate))]
			[HarmonyPostfix]
			public static void BeginCrewmatePatch(IntroCutscene __instance)
			{
				Game.PlayerTurn = 0;
				Game.TotalTurns = 0;
				int playerCount = GameData.Instance.PlayerCount;
				int[] colorIds = (int[])Game.ColorIds.GetValue(playerCount - 1);
				Game.AllPlayers.Clear();
				Game.AllCustomPlayers.Clear();
				if (Game.AllPlayers.Count == 0)
				{
					float timeAdded = Chess.Chess.MainTime != "Unlimited" ? float.Parse(Chess.Chess.MainTime) * 60 : float.MaxValue;
					for (int i = 0; i < colorIds.Length; i++)
					{
						for (int j = 0; j < PlayerControl.AllPlayerControls.Count; j++)
						{
							PlayerControl playerControl = PlayerControl.AllPlayerControls[j];
							if (playerControl.Data == null || colorIds[i] != playerControl.Data.ColorId) continue;
							Game.AllPlayers.Add(playerControl);
							CustomPlayer customPlayer = new CustomPlayer()
							{
								PlayerId = playerControl.PlayerId,
								HatId = playerControl.Data.HatId,
								SkinId = playerControl.Data.SkinId,
								PetId = playerControl.Data.PetId,
								Timer = timeAdded,
								Activity = EnumActivity.GameWaiting
							};
							Game.AllCustomPlayers.Add(customPlayer);
						}
					}
				}
				int color = PlayerControl.LocalPlayer.Data.ColorId;
				int index = -1;
				string otherTeams = "";
				for (int i = 0; i < Game.AllPlayers.Count; i++) if (Game.AllPlayers[i].PlayerId == PlayerControl.LocalPlayer.PlayerId) index = i;
				for (int i = 0; i < playerCount - 1; i++)
				{
					if (i == index) continue;
					otherTeams = otherTeams + ", " + Game.ColorNames[colorIds[i]].ToString().ToLower();
				}
				if (otherTeams.Length == 0)
				{
					otherTeams = "the other";
				}
				else
				{
					otherTeams = otherTeams[2..];
					int teamsIndex = otherTeams.LastIndexOf(',');
					if (teamsIndex != -1) _ = otherTeams.Insert(teamsIndex + 1, " and");
					if (teamsIndex != -1 && otherTeams.LastIndexOf(',') == otherTeams.IndexOf(',')) _ = otherTeams.Remove(teamsIndex);
					otherTeams += "'s";
				}
				__instance.Title.text = Game.ColorNames[colorIds[index]].ToString();
				__instance.Title.color = Palette.PlayerColors[color];
				__instance.ImpostorText.text = "Checkmate " + otherTeams + " king.";
				__instance.BackgroundBar.material.color = Palette.PlayerColors[color];
				Game.LocalActivity = playerCount == 1 ? EnumActivity.GameSelect : EnumActivity.GameWaiting;
				MessageWriter rpcMessageTime = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 71, (SendOption)1);
				rpcMessageTime.Write(PlayerControl.LocalPlayer.PlayerId);
				rpcMessageTime.EndMessage();
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
		public static class ShipStatusPatch
		{
			public static void Postfix()
			{
				HudManager.Instance.ShowTaskComplete();
				string shipDirectory = "PolusShip(Clone)/";
				string[] activeObjects = new string[] { "Storage", "Outside/ScienceBuildingVent", "Outside/ElectricBuildingVent", "Outside/SouthVent", "Comms/CommsVent", "LifeSupport/ElecFenceVent", "Science/SubBathroomVent", "Outside/panel_node_ca", "Outside/panel_node_tb", "Outside/panel_node_mlg", "Outside/panel_node_gi", "Outside/panel_node_iro", "Outside/panel_node_pd", "Outside/RocksNBoxes/bigRock" };
				string[] interactiveObjects = new string[] { "Outside/panel_temphot", "Dropship/panel_fuel", "Dropship/panel_fuel (1)", "Dropship/panel_keys", "Dropship/panel_nav" };
				string[] doorObjects = new string[] { "Comms/Walls/BottomDoor", "Weapons/Walls/BottomDoor", "LifeSupport/BottomDoor", "Electrical/RightDoor", "Office/RightDoor", "Admin/LeftDoor", "Science/RightDoor" };
				for (int i = 0; i < activeObjects.Length; i++)
				{
					GameObject.Find(shipDirectory + activeObjects[i]).active = false;
				}
				for (int i = 0; i < interactiveObjects.Length; i++)
				{
					GameObject.Find(shipDirectory + interactiveObjects[i]).GetComponent<BoxCollider2D>().enabled = false;
				}
				for (int i = 0; i < doorObjects.Length; i++)
				{
					PlainDoor plainDoor = GameObject.Find(shipDirectory + doorObjects[i]).GetComponent<PlainDoor>();
					plainDoor.Open = true;
					plainDoor.SetDoorway(false);
					Vector2 size = plainDoor.myCollider.size;
					if (size.x > size.y)
					{
						size.x = 0.7f;
						size.y = 1.5f;
					}
					else
					{
						size.x = 0.4f;
						size.y = 2f;
					}
					plainDoor.myCollider.size = new Vector2(size.x, size.y);
				}
				GameObject ventPath = new GameObject("VentPath");
				GameObject piecesPath = new GameObject("PiecesPath");
				Chess.Chess.ChessBoard = new char[,] { { '0' } };
				Chess.Chess.SetSettings();
				char[,] chessBoard = Chess.Utils.ReadableBoard(Chess.Chess.ChessBoard);
				int[] allColors = (int[])Game.ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
				for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < 8; x++)
					{
						Vent ventPrefab = Object.FindObjectOfType<Vent>();
						Vent ventControl = Object.Instantiate(ventPrefab, ventPath.transform);
						ventControl.transform.position = new Vector3((x * 0.5f) + 16, (y * -0.5f) - 10.31f, ventPrefab.transform.position.z);
						ventControl.name = x.ToString() + "," + y.ToString();
						if (chessBoard[y, x] == '1') continue;
						int pieceIndex = Utils.PieceIndex(chessBoard[y, x]);
						PlayerControl playerPrefab = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, piecesPath.transform);
						PlayerControl playerControl = playerPrefab.gameObject.GetComponent<PlayerControl>();
						playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();
						playerControl.isDummy = true;
						playerControl.transform.position = new Vector3((x * 0.5f) + 16, (y * -0.5f) - 10, PlayerControl.LocalPlayer.transform.position.z);
						playerControl.GetComponent<DummyBehaviour>().enabled = true;
						playerControl.NetTransform.enabled = false;
						playerControl.SetName(chessBoard[y, x] + ":" + x.ToString() + "," + y.ToString(), false);
						playerControl.nameText.color = new Color(1f, 1f, 1f, 0f);
						int team = char.IsUpper(chessBoard[y, x]) ? 0 : 1;
						if (team > allColors.Length - 1) team = 0;
						playerControl.scannerCount = (byte)team;
						playerControl.SetColor(allColors[team]);
						playerControl.SetHat(Utils.PieceHats[pieceIndex], allColors[team]);
						playerControl.SetSkin(Utils.PieceSkins[pieceIndex]);
						playerControl.SetPet(0u);
					}
				}
			}
		}

		[HarmonyPatch(typeof(PlainDoor))]
		public static class PlainDoorPatch
		{
			[HarmonyPatch(nameof(PlainDoor.Start))]
			[HarmonyPostfix]
			public static void Start(PlainDoor __instance)
			{
				__instance.Open = true;
			}
		}
	}
}
