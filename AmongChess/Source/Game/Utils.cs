using HarmonyLib;
using Hazel;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace AmongChess.Source.Game
{
	internal class Utils
	{
		public static char[] PieceTranslation = new char[6] { 'P', 'N', 'B', 'R', 'Q', 'K' };
		public static uint[] PieceHats = new uint[6] { 0u, 43u, 48u, 59u, 32u, 30u };
		public static uint[] PieceSkins = new uint[6] { 0u, 7u, 11u, 15u, 6u, 8u };

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

		public static PlayerControl FindPlayer(byte playerID)
		{
			for (int i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
			{
				PlayerControl playerControl = PlayerControl.AllPlayerControls[i];
				if (playerControl.PlayerId == playerID) return playerControl;
			}
			return null;
		}

		public static CustomPlayer FindCustom(byte playerID)
		{
			for (int i = 0; i < Game.AllCustomPlayers.Count; i++)
			{
				CustomPlayer playerControl = Game.AllCustomPlayers[i];
				if (playerControl.PlayerId == playerID) return playerControl;
			}
			return null;
		}

		public static int PieceIndex(char piece)
		{
			return Array.IndexOf(PieceTranslation, char.ToUpper(piece));
		}

		public static void RevertClothing(int index)
		{
			CustomPlayer customPlayer = Game.AllCustomPlayers[index];
			PlayerControl playerControl = Game.AllPlayers[index];
			playerControl.SetHat(customPlayer.HatId, playerControl.Data.ColorId);
			playerControl.SetSkin(customPlayer.SkinId);
			playerControl.SetPet(customPlayer.PetId);
		}

		public static void RevertClothingById(byte playerId)
		{
			PlayerControl playerControl = FindPlayer(playerId);
			int index = Game.AllPlayers.FindIndex(ele => ele.PlayerId == playerControl.PlayerId);
			RevertClothing(index);
		}

		public static void IncrementTurn()
		{
			Game.PlayerTurn++;
			if (Game.AllPlayers.Count <= Game.PlayerTurn)
			{
				Game.TotalTurns++;
				Game.PlayerTurn = 0;
			}
			if (Game.AllPlayers[Game.PlayerTurn].PlayerId == PlayerControl.LocalPlayer.PlayerId)
			{
				Game.LocalActivity = EnumActivity.GameSelect;
				HudManager.Instance.ShowTaskComplete();
			}
			else
			{
				Game.LocalActivity = EnumActivity.GameWaiting;
			}
		}

		public static void AddIncrementTime(int index)
		{
			Game.AllCustomPlayers[index].Timer += 0.25f + float.Parse(Chess.Chess.IncrementTime);
		}

		public static void SynchronizeTime(float timer)
		{
			MessageWriter rpcMessageTime = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 70, (SendOption)1);
			rpcMessageTime.Write(timer);
			rpcMessageTime.EndMessage();
		}

		public static void SendCoordinates((int x, int y) fromCoordinates, (int x, int y) toCoordinates)
		{
			MessageWriter rpcMessageMove = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 64, (SendOption)1);
			rpcMessageMove.Write((byte)fromCoordinates.x);
			rpcMessageMove.Write((byte)fromCoordinates.y);
			rpcMessageMove.Write((byte)toCoordinates.x);
			rpcMessageMove.Write((byte)toCoordinates.y);
			rpcMessageMove.EndMessage();
		}

		public static void RevertMove(int playerIndex, PlayerControl oldPlayer)
		{
			RevertClothing(playerIndex);
			MessageWriter rpcMessageLocal = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 66, (SendOption)1);
			rpcMessageLocal.Write(PlayerControl.LocalPlayer.PlayerId);
			rpcMessageLocal.EndMessage();
			oldPlayer.gameObject.active = true;
			oldPlayer.name = oldPlayer.name[1..];
			Game.LocalActivity = EnumActivity.GameSelect;
		}

		public static int FindIndexById(int playerId)
		{
			int playerIndex = -1;
			for (int i = 0; i < Game.AllPlayers.Count; i++)
			{
				if (Game.AllPlayers[i].PlayerId == playerId) playerIndex = i;
			}
			return playerIndex;
		}
	}
}
