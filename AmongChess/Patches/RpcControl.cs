using HarmonyLib;
using Hazel;

namespace AmongChess.Patches
{
	class RpcControl
	{
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
		public static class HandleRpcPatch
		{
			public static void Postfix(byte callId, MessageReader reader)
			{
				UnityEngine.Debug.logger.Log($"Call ID: {callId}");
				switch (callId)
				{
					case 64: // Move Piece
					{
						(byte x, byte y) fromCoordinates = (0, 0);
						(byte x, byte y) toCoordinates = (0, 0);
						fromCoordinates.x = reader.ReadByte();
						fromCoordinates.y = reader.ReadByte();
						toCoordinates.x = reader.ReadByte();
						toCoordinates.y = reader.ReadByte();
						GameEvents.Timers[GameEvents.PlayerTurn] += 0.25f + float.Parse(ChessControl.IncrementTime);
						GameEvents.PlayMove(fromCoordinates, toCoordinates);
						char[,] chessBoard = ChessControl.PlayMove(fromCoordinates, toCoordinates);
						GameEvents.IncrementTurn();
						ChessControl.ChessBoard = chessBoard;
						break;
					}
					case 65: // Select Piece
					{
						byte playerId = reader.ReadByte();
						byte selectedPiece = reader.ReadByte();
						PlayerControl playerControl = GameEvents.FindPlayer(playerId);
						playerControl.SetHat(GameEvents.HatTranslation[selectedPiece], playerControl.Data.ColorId);
						playerControl.SetSkin(GameEvents.SkinTranslation[selectedPiece]);
						playerControl.SetPet(0u);
						break;
					}
					case 66: // Return Piece
					{
						byte playerId = reader.ReadByte();
						byte hatId = reader.ReadByte(); // Works for now, change to uint if there are more than 255 hats.
						byte skinId = reader.ReadByte();
						byte petId = reader.ReadByte();
						PlayerControl playerControl = GameEvents.FindPlayer(playerId);
						playerControl.SetHat(hatId, playerControl.Data.ColorId);
						playerControl.SetSkin(skinId);
						playerControl.SetPet(petId);
						break;
					}
					case 67: // Game Ends
					{
						byte winEvent = reader.ReadByte();
						GameEvents.EventEnded(winEvent == 0 ? 'S' : 'C', false);
						break;
					}
					case 68: // Custom Options Retrieve
					{
						if (OptionControl.AllOption.Count == 0)
						{
							OptionControl.AllOption = OptionControl.OptionDefault();
							OptionControl.AllOptionGroup = OptionControl.OptionGroupDefault();
						}
						while (reader.BytesRemaining > 0)
						{
							byte optionId = reader.ReadByte();
							OptionSingle optionSingle = OptionControl.AllOption.Find(option => option.Id == optionId);
							optionSingle.Value = reader.ReadByte();
						}
						break;
					}
					case 69: // Tell owner the game ended
					{
						if (PlayerControl.LocalPlayer.AmOwner)
						{
							ShipStatus.RpcEndGame(GameOverReason.ImpostorByVote, false);
						}
						break;
					}
				}
			}
		}
	}
}
