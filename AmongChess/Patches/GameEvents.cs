using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmongChess.Patches
{
	class GameEvents
	{
		public static List<PlayerControl> AllPlayers = new List<PlayerControl> { };
		public static char[] PieceTranslation = new char[6] { 'p', 'n', 'b', 'r', 'q', 'k' };
		public static uint[] HatTranslation = new uint[6] { 0u, 43u, 48u, 59u, 32u, 30u };
		public static uint[] SkinTranslation = new uint[6] { 0u, 7u, 11u, 15u, 6u, 8u };
		public static int[][] ColorIds = new int[2][] { new int[1] { 1 }, new int[2] { 7, 6 } };
		public static string[][] ColorNames = new string[2][] { new string[1] { "Blue" }, new string[2] { "White", "Black" } };
		public static char PlayerActivity = 's';
		public static List<float> Timers = new List<float> { };
		public static byte PlayerTurn = 0;
		public static uint TotalTurns = 0;
		public static uint LocalHat = 0;
		public static uint LocalSkin = 0;
		public static uint LocalPet = 0;

		[HarmonyPatch(typeof(ShipStatus))]
		public static class ShipStatusPatch
		{
			[HarmonyPatch(nameof(ShipStatus.RpcEndGame))]
			[HarmonyPrefix]
			public static bool RpcEndGamePatch(ref GameOverReason endReason)
			{
				if (endReason == GameOverReason.HumansByVote) return false;
				MessageWriter rpcMessage = AmongUsClient.Instance.StartEndGame();
				rpcMessage.Write((byte)GameOverReason.HumansByVote);
				rpcMessage.Write(false);
				AmongUsClient.Instance.FinishEndGame(rpcMessage);
				return false;
			}

			[HarmonyPatch(nameof(ShipStatus.Start))]
			[HarmonyPostfix]
			public static void StartPatch()
			{
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
				ChessControl.ChessBoard = new char[,] { { '0' } };
				ChessControl.SetSettings();
				char[,] chessBoard = ChessControl.ReadableBoard();
				int[] allColors = (int[])ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
				for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < 8; x++)
					{
						Vent ventPrefab = UnityEngine.Object.FindObjectOfType<Vent>();
						Vent ventControl = UnityEngine.Object.Instantiate(ventPrefab, ventPath.transform);
						ventControl.transform.position = new Vector3((x * 0.5f) + 16, (y * -0.5f) - 10.31f, ventPrefab.transform.position.z);
						ventControl.name = x.ToString() + "," + y.ToString();
						if (chessBoard[y, x] == '1') continue;
						int pieceIndex = Array.IndexOf(PieceTranslation, char.ToLower(chessBoard[y, x]));
						PlayerControl playerPrefab = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab, piecesPath.transform);
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
						playerControl.SetHat(HatTranslation[pieceIndex], allColors[team]);
						playerControl.SetSkin(SkinTranslation[pieceIndex]);
						playerControl.SetPet(0u);
					}
				}
			}

			[HarmonyPatch(nameof(ShipStatus.FixedUpdate))]
			[HarmonyPrefix]
			public static void FixedUpdatePatch()
			{
				if (PlayerActivity == 'E' || PlayerActivity == 's' || TotalTurns <= 0) return;
				TimeManagement();
			}
		}

		[HarmonyPatch(typeof(EndGameManager))]
		public static class EndGameManagerPatch
		{
			[HarmonyPatch(nameof(EndGameManager.Start))]
			[HarmonyPostfix]
			public static void StartPatch(EndGameManager __instance)
			{
				string[] colorNames = (string[])ColorNames.GetValue(GameData.Instance.PlayerCount - 1);
				int[] colorIds = (int[])ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
				__instance.WinText.text = colorNames[PlayerTurn] + " won";
				__instance.WinText.color = Palette.PlayerColors[colorIds[PlayerTurn]];
			}
		}

		[HarmonyPatch(typeof(IntroCutscene))]
		public static class IntroCutScenePatch
		{
			[HarmonyPatch(nameof(IntroCutscene.BeginCrewmate))]
			[HarmonyPostfix]
			public static void BeginCrewmatePatch(IntroCutscene __instance)
			{
				LocalHat = PlayerControl.LocalPlayer.Data.HatId;
				LocalSkin = PlayerControl.LocalPlayer.Data.SkinId;
				LocalPet = PlayerControl.LocalPlayer.Data.PetId;
				int playerCount = GameData.Instance.PlayerCount;
				string[] colorNames = (string[])ColorNames.GetValue(playerCount - 1);
				int[] colorIds = (int[])ColorIds.GetValue(playerCount - 1);
				if (AllPlayers.Count == 0)
				{
					for (int j = 0; j < PlayerControl.AllPlayerControls.Count; j++)
					{
						if (PlayerControl.AllPlayerControls[j].Data == null) continue;
						for (int i = 0; i < colorIds.Length; i++)
						{
							PlayerControl playerControl = PlayerControl.AllPlayerControls[j];
							if (colorIds[i] == PlayerControl.AllPlayerControls[j].Data.ColorId) AllPlayers.Add(PlayerControl.AllPlayerControls[j]);
						}
					}
				}
				for (int i = 0; i < AllPlayers.Count; i++)
				{
					AllPlayers[i].SetColor(colorIds[i]);
				}
				int color = PlayerControl.LocalPlayer.Data.ColorId;
				int index = -1;
				string otherTeams = "";
				for (int i = 0; i < AllPlayers.Count; i++) if (AllPlayers[i].PlayerId == PlayerControl.LocalPlayer.PlayerId) index = i;
				for (int i = 0; i < playerCount - 1; i++)
				{
					if (i == index) continue;
					otherTeams = otherTeams + ", " + colorNames[i].ToLower();
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
				__instance.Title.text = colorNames[index];
				__instance.Title.color = Palette.PlayerColors[color];
				__instance.ImpostorText.text = "Checkmate " + otherTeams + " king.";
				__instance.BackgroundBar.material.color = Palette.PlayerColors[color];
				int[] allColors = (int[])ColorIds.GetValue(playerCount - 1);
				PlayerActivity = PlayerControl.LocalPlayer.Data.ColorId == allColors[0] ? 'I' : 'W';
				float timeAdded = ChessControl.MainTime != "Unlimited" ? float.Parse(ChessControl.MainTime) * 60 : float.MaxValue;
				Timers = new List<float> { };
				for (int i = 0; i < playerCount; i++)
				{
					Timers.Add(timeAdded);
				}
				PlayerTurn = 0;
				TotalTurns = 0;
			}
		}

		[HarmonyPatch(typeof(PlayerControl))]
		public static class PlayerControlPatch
		{
			[HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
			[HarmonyPrefix]
			public static void Start(PlayerControl __instance)
			{
				__instance.Visible = true;
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

		[HarmonyPatch(typeof(TaskPanelBehaviour))]
		public static class TaskPanelBehaviourPatch
		{
			private static GameObject progressTracker = null;
			private static GameObject tab = null;
			private static GameObject background = null;

			private static string GameOptionHudLayer1(string Name, string Value)
			{
				return "<color=#FFFFFF>┃\n┣</color> <color=#BBBBFF>" + Name + ": " + Value + "\n</color>";
			}

			private static string GameOptionHudLayer2(string Name, string Value)
			{
				return "<color=#FFFFFF>┃</color> <color=#BBBBFF>┃</color>\n<color=#FFFFFF>┃</color> <color=#BBBBFF>┣</color> <color=#7777FF>" + Name + ": " + Value + "\n</color>";
			}

			[HarmonyPatch(nameof(TaskPanelBehaviour.Update))]
			[HarmonyPrefix]
			public static bool UpdatePatch(TaskPanelBehaviour __instance)
			{
				if (progressTracker == null) progressTracker = __instance.transform.parent.FindChild("ProgressTracker").gameObject;
				if (tab == null) tab = __instance.transform.FindChild("Tab").gameObject;
				if (background == null) background = __instance.transform.FindChild("Background").gameObject;
				progressTracker.active = false;
				tab.active = false;
				background.active = false;
				__instance.transform.localPosition = new Vector3(-5.25f, 3f, 0f);
				int playerCount = GameData.Instance.PlayerCount;
				string[] colorNames = (string[])ColorNames.GetValue(playerCount - 1);
				OptionSingle optionMainTime = OptionControl.AllOption.Find(ele => ele.Id == 3);
				OptionSingle optionIncrementTime = OptionControl.AllOption.Find(ele => ele.Id == 4);
				string timeControl = optionMainTime.AllValues[optionMainTime.Value] + " + " + optionIncrementTime.AllValues[optionIncrementTime.Value];
				string results = "Among Chess Details\n";
				results += GameOptionHudLayer1("Turn", colorNames[PlayerTurn]);
				results += GameOptionHudLayer1("Time", timeControl);
				for (int i = 0; i < playerCount; i++)
				{
					if (Timers.Count == 0) break;
					float timer = Timers[i];
					if (timer == float.MaxValue) break;
					TimeSpan time = TimeSpan.FromSeconds(timer);
					string format = time.TotalHours >= 1 ? time.ToString(@"hh\:mm") : (time.TotalMinutes >= 1 ? time.ToString(@"mm\:ss") : time.ToString(@"ss\:ff"));
					results += GameOptionHudLayer2(colorNames[i], format);
				}
				results += GameOptionHudLayer1("Moves", TotalTurns.ToString());
				results = "<line-height=2>" + results + "</line-height>";
				results = "<size=2>" + results + "</size>";
				__instance.TaskText.text = results;
				return false;
			}

			[HarmonyPatch(nameof(TaskPanelBehaviour.ToggleOpen))]
			[HarmonyPrefix]
			public static bool ToggleOpenPatch(TaskPanelBehaviour __instance)
			{
				__instance.open = false;
				return false;
			}
		}

		public static PlayerControl ClosestPiece(PlayerControl referencePlayer, int color, out float minDistance)
		{
			GameObject allObjects = GameObject.Find("PiecesPath");
			minDistance = float.MaxValue;
			PlayerControl result = referencePlayer;
			for (int i = 0; i < allObjects.transform.childCount; i++)
			{
				GameObject elementObject = allObjects.transform.GetChild(i).gameObject;
				PlayerControl elementPlayer = elementObject.GetComponent<PlayerControl>();
				if (color != elementPlayer.scannerCount || elementPlayer.name.IndexOf(",") == -1) continue;
				float distance = Vector2.Distance(referencePlayer.GetTruePosition(), elementPlayer.GetTruePosition());
				if (minDistance < distance) continue;
				minDistance = distance;
				result = elementPlayer;
			}
			return result;
		}

		public static Vent ClosestVent(PlayerControl referencePlayer, out float minDistance)
		{
			GameObject allObjects = GameObject.Find("VentPath");
			minDistance = float.MaxValue;
			Vent result = null;
			for (int i = 0; i < allObjects.transform.childCount; i++)
			{
				GameObject elementObject = allObjects.transform.GetChild(i).gameObject;
				Vent elementPlayer = elementObject.GetComponent<Vent>();
				float distance = Vector2.Distance(referencePlayer.GetTruePosition(), new Vector2(elementObject.transform.position.x, elementObject.transform.position.y - 0.1f));
				if (minDistance < distance) continue;
				minDistance = distance;
				result = elementPlayer;
			}
			return result;
		}

		[HarmonyPatch(typeof(UseButtonManager))]
		public static class UseButtonManagerPatch
		{
			public static int num = 0;

			[HarmonyPatch(nameof(UseButtonManager.SetTarget))]
			[HarmonyPrefix]
			public static bool SetTarget(UseButtonManager __instance)
			{
				if (PlayerActivity == 's')
				{
					return true;
				}
				else if (PlayerActivity == 'E')
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
					if (PlayerActivity == 'I')
					{
						int[] colorIds = (int[])ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
						int colorId = 0;
						for (int i = 0; i < colorIds.Length; i++) if (colorIds[i] == PlayerControl.LocalPlayer.Data.ColorId) { colorId = i; break; };
						PlayerControl target = ClosestPiece(PlayerControl.LocalPlayer, colorId, out float distance);
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
					else if (PlayerActivity == 'O')
					{
						Vent target = ClosestVent(PlayerControl.LocalPlayer, out float distance);
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
				if (PlayerActivity == 's')
				{
					return true;
				}
				else if (PlayerActivity == 'O')
				{
					Vent target = ClosestVent(PlayerControl.LocalPlayer, out float distance);
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
					if (targetCoordinates.x == pieceCoordinates.x && targetCoordinates.y == pieceCoordinates.y)
					{
						localPlayer.SetHat(LocalHat, localPlayer.Data.ColorId);
						localPlayer.SetSkin(LocalSkin);
						localPlayer.SetPet(LocalPet);
						MessageWriter rpcMessageLocal = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 66, (SendOption)1);
						rpcMessageLocal.Write(localPlayer.PlayerId);
						rpcMessageLocal.Write((byte)LocalHat);
						rpcMessageLocal.Write((byte)LocalSkin);
						rpcMessageLocal.Write((byte)LocalPet);
						rpcMessageLocal.EndMessage();
						oldPlayer.gameObject.active = true;
						oldPlayer.name = oldPlayer.name[1..];
						PlayerActivity = 'I';
						return false;
					}
					char move = ChessControl.MovePiece(pieceCoordinates, targetCoordinates, oldPlayer.gameObject);
					if (move == 'e') return false;
					localPlayer.SetHat(LocalHat, localPlayer.Data.ColorId);
					localPlayer.SetSkin(LocalSkin);
					localPlayer.SetPet(LocalPet);
					MessageWriter rpcMessageMove = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 64, (SendOption)1);
					rpcMessageMove.Write((byte)pieceCoordinates.x);
					rpcMessageMove.Write((byte)pieceCoordinates.y);
					rpcMessageMove.Write((byte)targetCoordinates.x);
					rpcMessageMove.Write((byte)targetCoordinates.y);
					rpcMessageMove.EndMessage();
					MessageWriter rpcMessageReturn = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 66, (SendOption)1);
					rpcMessageReturn.Write(localPlayer.PlayerId);
					rpcMessageReturn.Write((byte)LocalHat);
					rpcMessageReturn.Write((byte)LocalSkin);
					rpcMessageReturn.Write((byte)LocalPet);
					rpcMessageReturn.EndMessage();
					target.GetComponent<SpriteRenderer>().GetMaterial().SetFloat("_Outline", 0);
					int pieceIndex = Array.IndexOf(PieceTranslation, char.ToLower(target.name[0]));
					oldPlayer.gameObject.active = true;
					Timers[PlayerTurn] += 0.25f + float.Parse(ChessControl.IncrementTime);
					IncrementTurn();
					if (move == 'C' || move == 'S')
					{
						EventEnded(move);
						return false;
					}
				}
				else if (PlayerActivity == 'I')
				{
					int[] colorIds = (int[])ColorIds.GetValue(GameData.Instance.PlayerCount - 1);
					int colorId = 0;
					for (int i = 0; i < colorIds.Length; i++) if (colorIds[i] == PlayerControl.LocalPlayer.Data.ColorId) { colorId = i; break; };
					PlayerControl targetPlayer = ClosestPiece(PlayerControl.LocalPlayer, colorId, out float distance);
					if (distance > 1) return false;
					PlayerControl localPlayer = PlayerControl.LocalPlayer;
					targetPlayer.transform.FindChild("Sprite").GetComponent<SpriteRenderer>().GetMaterial().SetFloat("_Outline", 0);
					int pieceIndex = Array.IndexOf(PieceTranslation, char.ToLower(targetPlayer.name[0]));
					targetPlayer.gameObject.active = false;
					localPlayer.transform.position = targetPlayer.transform.position;
					localPlayer.SetHat(HatTranslation[pieceIndex], localPlayer.Data.ColorId);
					localPlayer.SetSkin(SkinTranslation[pieceIndex]);
					localPlayer.SetPet(0u);
					MessageWriter rpcMessage = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 65, (SendOption)1);
					rpcMessage.Write(localPlayer.PlayerId);
					rpcMessage.Write((byte)pieceIndex);
					rpcMessage.EndMessage();
					targetPlayer.name = "t" + targetPlayer.name;
					PlayerActivity = 'O';
				}
				else if (PlayerActivity == 'E')
				{
					ShipStatus.RpcEndGame(GameOverReason.ImpostorByVote, false);
				}
				else if (PlayerActivity == 'W')
				{
					return false;
				}

				return false;
			}
		}

		public static void PlayMove(GameObject fromObject, (int x, int y) toCoordinates, GameObject toObject, char howMove, int captures)
		{
			PlayerControl fromController = fromObject.GetComponent<PlayerControl>();
			PlayerControl toController = toObject.GetComponent<PlayerControl>();
			int nameIndexFrom = fromObject.name.IndexOf(':');
			int nameIndexTo = toObject.name.IndexOf(':');
			int team = char.IsUpper(fromObject.name[nameIndexFrom - 1]) ? 1 : 0;
			if (howMove == 'P')
			{
				int pieceIndex = 4;
				fromController.SetHat(HatTranslation[pieceIndex], fromController.scannerCount);
				fromController.SetSkin(SkinTranslation[pieceIndex]);
				fromController.SetName((team == 1 ? "Q" : "q") + fromObject.name[nameIndexFrom..]);
				nameIndexFrom = fromController.name.IndexOf(':');
			}
			switch (howMove)
			{
				case 'K':
				case 'Q':
					fromController.SetName(fromObject.name[nameIndexFrom - 1] + ":" + (howMove == 'Q' ? "2" : "6") + "," + toCoordinates.y.ToString());
					toController.SetName(toObject.name[nameIndexTo - 1] + ":" + (howMove == 'Q' ? "3" : "5") + "," + toCoordinates.y.ToString());
					fromObject.transform.position = new Vector3(howMove == 'Q' ? 17f : 19f, (toCoordinates.y * -0.5f) - 10f, fromObject.transform.position.z);
					toObject.transform.position = new Vector3(howMove == 'Q' ? 17.5f : 18.5f, (toCoordinates.y * -0.5f) - 10f, toObject.transform.position.z);
					break;
				case 'E':
					fromController.SetName(fromObject.name[nameIndexFrom - 1] + ":" + toCoordinates.x.ToString() + "," + toCoordinates.y.ToString());
					toController.SetName(toObject.name[nameIndexTo - 1] + ":D");
					fromObject.transform.position = new Vector3((toCoordinates.x * 0.5f) + 16, (toCoordinates.y * -0.5f) - 10f, fromObject.transform.position.z);
					toObject.transform.position = new Vector3(25f + (captures % 10 * 0.5f), (float)((team == 1 ? -12f : -14.5f) + (Math.Floor(captures / 10f) * (team == 1 ? -0.5f : 0.5f))), toObject.transform.position.z);
					break;
				default:
					fromController.SetName(fromObject.name[nameIndexFrom - 1] + ":" + toCoordinates.x.ToString() + "," + toCoordinates.y.ToString());
					toController.SetName(toObject.name[nameIndexTo - 1] + ":D");
					fromObject.transform.position = new Vector3((toCoordinates.x * 0.5f) + 16, (toCoordinates.y * -0.5f) - 10f, fromObject.transform.position.z);
					toObject.transform.position = new Vector3(25f + (captures % 10 * 0.5f), (float)((team == 1 ? -12f : -14.5f) + (Math.Floor(captures / 10f) * (team == 1 ? -0.5f : 0.5f))), toObject.transform.position.z);
					break;
			}
		}

		public static void PlayMove(GameObject fromObject, (int x, int y) toCoordinates, char howMove)
		{
			PlayerControl fromController = fromObject.GetComponent<PlayerControl>();
			int nameIndex = fromController.name.IndexOf(':');
			if (howMove == 'P')
			{
				int pieceIndex = 4;
				fromController.SetHat(HatTranslation[pieceIndex], fromController.scannerCount);
				fromController.SetSkin(SkinTranslation[pieceIndex]);
				fromController.SetName((char.IsUpper(fromObject.name[nameIndex - 1]) ? "Q" : "q") + fromObject.name[nameIndex..]);
				nameIndex = fromController.name.IndexOf(':');
			}
			fromController.SetName(fromObject.name[nameIndex - 1] + ":" + toCoordinates.x.ToString() + "," + toCoordinates.y.ToString());
			fromObject.transform.position = new Vector3((toCoordinates.x * 0.5f) + 16, (toCoordinates.y * -0.5f) - 10, fromObject.transform.position.z);
		}

		public static void PlayMove((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			GameObject fromObject = null;
			char howMove = 'N';
			int directionY = char.IsUpper(ChessControl.ChessBoard[fromCoordinates.y, fromCoordinates.x]) ? -1 : 1;
			float halfRank = ChessControl.ChessBoard.GetLength(0) * 0.5f;
			if (ChessControl.ReadablePiece(ChessControl.ChessBoard[fromCoordinates.y, fromCoordinates.x]) == 'P' && toCoordinates.y == halfRank + (halfRank * directionY))
			{
				howMove = 'P';
			}
			Transform piecesObject = GameObject.Find("PiecesPath").transform;
			for (int i = 0; i < piecesObject.childCount; i++)
			{
				Transform elementObject = piecesObject.GetChild(i);
				int pieceNameIndex1 = elementObject.name.IndexOf(':');
				int pieceNameIndex2 = elementObject.name.IndexOf(',');
				if (pieceNameIndex1 == -1 || pieceNameIndex2 == -1) continue;
				(int x, int y) elementCoordinates = (int.Parse(elementObject.name[(pieceNameIndex1 + 1)..pieceNameIndex2]), int.Parse(elementObject.name[(pieceNameIndex2 + 1)..]));
				if (fromCoordinates.x == elementCoordinates.x && fromCoordinates.y == elementCoordinates.y)
				{
					fromObject = elementObject.gameObject.GetComponent<PlayerControl>().gameObject;
					break;
				}
			}
			if (ChessControl.ChessBoard[toCoordinates.y, toCoordinates.x] != '1')
			{
				howMove = ChessControl.GetHowMove(fromCoordinates, toCoordinates);
				int captures = char.IsUpper(ChessControl.ChessBoard[fromCoordinates.y, fromCoordinates.x]) ? ChessControl.numCaptures.black : ChessControl.numCaptures.white;
				GameObject toObject = PlayerControl.LocalPlayer.gameObject;
				for (int i = 0; i < piecesObject.childCount; i++)
				{
					Transform elementObject = piecesObject.GetChild(i);
					int pieceNameIndex = elementObject.name.IndexOf(':');
					if (elementObject.name[(pieceNameIndex + 1)..] == toCoordinates.x + "," + toCoordinates.y)
					{
						toObject = elementObject.gameObject;
						break;
					}
				}
				PlayMove(fromObject, toCoordinates, toObject, howMove, captures++);
			}
			else
			{
				PlayMove(fromObject, toCoordinates, howMove);
			}
		}

		public static void EventEnded(char results)
		{
			MessageWriter rpcMessageLocal = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 67, (SendOption)1);
			rpcMessageLocal.Write((byte)(results == 'S' ? 0 : 1));
			rpcMessageLocal.EndMessage();
			PlayerActivity = 'E';
			if (PlayerTurn == 0) PlayerTurn = (byte)AllPlayers.Count;
			PlayerTurn--;
			string[] colorNames = (string[])ColorNames.GetValue(GameData.Instance.PlayerCount - 1);
			if (results == 'S') HudManager.Instance.ShowPopUp("After " + TotalTurns.ToString() + " turn" + (TotalTurns == 1 ? "" : "s") + ", the game ended as a stalemate.");
			else HudManager.Instance.ShowPopUp("After " + TotalTurns.ToString() + " turn" + (TotalTurns == 1 ? "" : "s") + ", " + colorNames[PlayerTurn] + " won the game. Click the \"Use Button\" to travel back to the lobby.");
			ClearAllHighlighted();
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

		public static PlayerControl FindPlayer(byte playerID)
		{
			for (int i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
			{
				PlayerControl playerControl = PlayerControl.AllPlayerControls[i];
				if (playerControl.PlayerId == playerID) return playerControl;
			}
			return null;
		}

		public static void IncrementTurn()
		{
			PlayerTurn++;
			if (AllPlayers.Count <= PlayerTurn)
			{
				TotalTurns++;
				PlayerTurn = 0;
			}
			PlayerActivity = AllPlayers[PlayerTurn].PlayerId == PlayerControl.LocalPlayer.PlayerId ? 'I' : 'W';
		}

		public static void TimeManagement()
		{
			Timers[PlayerTurn] -= Time.fixedDeltaTime;
			if (0 >= Timers[PlayerTurn])
			{
				Timers[PlayerTurn] = 0f;
				EventEnded('T');
			}
		}
	}
}