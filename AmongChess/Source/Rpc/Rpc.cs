using HarmonyLib;
using Hazel;

namespace AmongChess.Source.Rpc
{
	internal class Rpc
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
						Game.Utils.AddIncrementTime(Game.Game.PlayerTurn);
						Game.Game.GetAndPlayMove(fromCoordinates, toCoordinates);
						char[,] chessBoard = Chess.Chess.PlayMove(fromCoordinates, toCoordinates);
						Game.Utils.IncrementTurn();
						Chess.Chess.ChessBoard = chessBoard;
						break;
					}
					case EnumRpc.SelectPiece:
					{
						byte playerId = reader.ReadByte();
						byte selectedPiece = reader.ReadByte();
						PlayerControl playerControl = Game.Utils.FindPlayer(playerId);
						playerControl.SetHat(Game.Utils.PieceHats[selectedPiece], playerControl.Data.ColorId);
						playerControl.SetSkin(Game.Utils.PieceSkins[selectedPiece]);
						playerControl.SetPet(0u);
						break;
					}
					case EnumRpc.ReturnPiece:
					{
						byte playerId = reader.ReadByte();
						Game.Utils.RevertClothingById(playerId);
						break;
					}
					case EnumRpc.GameResult:
					{
						byte winEvent = reader.ReadByte();
						byte winnerId = reader.ReadByte();
						Game.Game.EventEnded((Chess.EnumResults)winEvent, winnerId, false);
						break;
					}
					case EnumRpc.CustomOptions:
					{
						if (Lobby.Options.AllOption.Count == 0)
						{
							Lobby.Options.AllOption = Lobby.Options.OptionDefault();
							Lobby.Options.AllOptionGroup = Lobby.Options.OptionGroupDefault();
						}
						while (reader.BytesRemaining > 0)
						{
							byte optionId = reader.ReadByte();
							Lobby.ClassOption optionSingle = Lobby.Options.AllOption.Find(option => option.Id == optionId);
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
						Game.Game.AllCustomPlayers[Game.Game.PlayerTurn].Timer = time;
						break;
					}
					case EnumRpc.PlayerLoaded:
					{
						if (AmongUsClient.Instance.AmHost)
						{
							byte playerId = reader.ReadByte();
							Game.Utils.FindCustom(playerId).Loaded = true;
							Game.Utils.FindCustom(PlayerControl.LocalPlayer.PlayerId).Loaded = true;
							if (Game.Game.AllCustomPlayers.TrueForAll(ele => ele.Loaded == true))
							{
								int[] colorIds = (int[])Game.Game.ColorIds.GetValue(Game.Game.AllPlayers.Count - 1);
								Game.Game.LocalActivity = PlayerControl.LocalPlayer.Data.ColorId == colorIds[0] ? Game.EnumActivity.GameSelect : Game.EnumActivity.Lobby;
								MessageWriter rpcMessageTime = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 72, (SendOption)1);
								rpcMessageTime.EndMessage();
							}
						}
						break;
					}
					case EnumRpc.GameStart:
					{
						int[] colorIds = (int[])Game.Game.ColorIds.GetValue(Game.Game.AllPlayers.Count - 1);
						Game.Game.LocalActivity = PlayerControl.LocalPlayer.Data.ColorId == colorIds[0] ? Game.EnumActivity.GameSelect : Game.EnumActivity.Lobby;
						break;
					}
				}
			}
		}
	}
}