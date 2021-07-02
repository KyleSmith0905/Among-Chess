using HarmonyLib;
using Hazel;

namespace AmongChess.Patches
{
	internal class RpcControl
	{
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
		public static class HandleRpcPatch
		{
			public static void Postfix(byte callId, MessageReader reader)
			{
				UnityEngine.Debug.logger.Log("CallId: " + callId.ToString());
				switch ((EnumRpc)callId)
				{
					case EnumRpc.MovePiece:
					{
						(byte x, byte y) fromCoordinates = (0, 0);
						(byte x, byte y) toCoordinates = (0, 0);
						fromCoordinates.x = reader.ReadByte();
						fromCoordinates.y = reader.ReadByte();
						toCoordinates.x = reader.ReadByte();
						toCoordinates.y = reader.ReadByte();
						GameControl.Utilities.AddIncrementTime(GameControl.Control.PlayerTurn);
						GameControl.Control.GetAndPlayMove(fromCoordinates, toCoordinates);
						char[,] chessBoard = ChessControl.Control.PlayMove(fromCoordinates, toCoordinates);
						GameControl.Utilities.IncrementTurn();
						ChessControl.Control.ChessBoard = chessBoard;
						break;
					}
					case EnumRpc.SelectPiece:
					{
						byte playerId = reader.ReadByte();
						byte selectedPiece = reader.ReadByte();
						PlayerControl playerControl = GameControl.Utilities.FindPlayer(playerId);
						playerControl.SetHat(GameControl.Utilities.PieceHats[selectedPiece], playerControl.Data.ColorId);
						playerControl.SetSkin(GameControl.Utilities.PieceSkins[selectedPiece]);
						playerControl.SetPet(0u);
						break;
					}
					case EnumRpc.ReturnPiece:
					{
						byte playerId = reader.ReadByte();
						GameControl.Utilities.RevertClothingById(playerId);
						break;
					}
					case EnumRpc.GameResult:
					{
						byte winEvent = reader.ReadByte();
						byte winnerId = reader.ReadByte();
						GameControl.Control.EventEnded((ChessControl.EnumResults)winEvent, winnerId, false);
						break;
					}
					case EnumRpc.CustomOptions:
					{
						if (LobbyControl.OptionControl.AllOption.Count == 0)
						{
							LobbyControl.OptionControl.AllOption = LobbyControl.OptionControl.OptionDefault();
							LobbyControl.OptionControl.AllOptionGroup = LobbyControl.OptionControl.OptionGroupDefault();
						}
						while (reader.BytesRemaining > 0)
						{
							byte optionId = reader.ReadByte();
							OptionSingle optionSingle = LobbyControl.OptionControl.AllOption.Find(option => option.Id == optionId);
							optionSingle.Value = reader.ReadByte();
						}
						break;
					}
					case EnumRpc.GameEnd:
					{
						if (AmongUsClient.Instance.AmHost)
						{
							ShipStatus.RpcEndGame(GameOverReason.ImpostorByVote, false);
						}
						break;
					}
					case EnumRpc.SynchronizeTime:
					{
						float time = reader.ReadSingle();
						GameControl.Control.AllCustomPlayers[GameControl.Control.PlayerTurn].Timer = time;
						break;
					}
					case EnumRpc.PlayerLoaded:
					{
						if (AmongUsClient.Instance.AmHost)
						{
							byte playerId = reader.ReadByte();
							GameControl.Utilities.FindCustom(playerId).Loaded = true;
							GameControl.Utilities.FindCustom(PlayerControl.LocalPlayer.PlayerId).Loaded = true;
							if (GameControl.Control.AllCustomPlayers.TrueForAll(ele => ele.Loaded == true))
							{
								int[] colorIds = (int[])GameControl.Control.ColorIds.GetValue(GameControl.Control.AllPlayers.Count - 1);
								GameControl.Control.LocalActivity = PlayerControl.LocalPlayer.Data.ColorId == colorIds[0] ? GameControl.EnumActivity.GameSelect : GameControl.EnumActivity.Lobby;
								MessageWriter rpcMessageTime = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 72, (SendOption)1);
								rpcMessageTime.EndMessage();
							}
						}
						break;
					}
					case EnumRpc.GameStart:
					{
						int[] colorIds = (int[])GameControl.Control.ColorIds.GetValue(GameControl.Control.AllPlayers.Count - 1);
						GameControl.Control.LocalActivity = PlayerControl.LocalPlayer.Data.ColorId == colorIds[0] ? GameControl.EnumActivity.GameSelect : GameControl.EnumActivity.Lobby;
						break;
					}
				}
			}
		}
	}
}