using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using System;
using System.Linq;

namespace AmongChess.Source.TestCases
{
	public class Tests
	{
		[HarmonyPatch(typeof(OptionsMenuBehaviour))]
		public class OptionsMenuBehaviourPatch
		{
			[HarmonyPatch(nameof(OptionsMenuBehaviour.OpenTabGroup))]
			[HarmonyPostfix]
			public static void OpenTabGroupPatch(int index)
			{
				if (index != 0) return;
				GameObject parentButton = GameObject.Find("OptionsMenu/GeneralTab/ChatGroup/");
				if (!parentButton || parentButton.transform.childCount != 1) return;
				GameObject chatButton = parentButton.transform.GetChild(0).gameObject;
				GameObject debugObject = UnityEngine.Object.Instantiate(chatButton, chatButton.transform.parent);
				debugObject.transform.position = new Vector3(debugObject.transform.position.x, -1.5f, debugObject.transform.position.z);
				GameObject debugText = debugObject.transform.FindChild("Text_TMP").gameObject;
				TextMeshPro debugTextMesh = debugText.GetComponent<TextMeshPro>();
				debugTextMesh.text = "Chess Debug";
			}

			[HarmonyPatch(nameof(OptionsMenuBehaviour.ToggleCensorChat))]
			[HarmonyPrefix]
			public static bool ToggleCensorChatPatch()
			{
				float positionRatio = Input.mousePosition.y / Screen.height;
				if (0.29f < positionRatio) return true;
				ActivateTests();
				return false;
			}
		}

		public static void ActivateTests()
		{
			Debug.logger.Log(" - - - - - ACTIVATING TESTS - - - - - ");
			int correct = 0;
			List<bool> allCases = TestCases();
			if (allCases.Count == 0)
			{
				Debug.logger.LogError("Test Failure", "No test results were outputted, see Among-Chess/AmongChess/Patches/TestCases/Control.cs");
			}
			for (int i = 0; i < allCases.Count; i++)
			{
				if (!allCases[i])
				{
					correct++;
					Debug.logger.Log("Test Case " + i.ToString() + ": TEST CASE FAILED !!! " + i.ToString());
				}
				else
				{
					Debug.logger.Log("Test Case " + i.ToString() + ": Test Case Working");
				}
			}
			float percentage = (float)(allCases.Count - correct) / allCases.Count;
			Debug.logger.Log("Percentage of test cases working: " + (percentage * 100f).ToString("0") + "%");
			Debug.logger.Log(" - - - - - TESTS COMPLETED - - - - - ");
		}

		public static List<bool> TestCases()
		{
			List<bool> allCases = new List<bool> { };
/* 0*/allCases.Add(Chess.Validation.IsInCheckmate((1, 1), Examples.ChessBoards[0]) == 'n');
/* 1*/allCases.Add(Chess.Validation.IsInCheckmate((1, 1), Examples.ChessBoards[1]) == 'e');
/* 2*/allCases.Add(Chess.Validation.IsInCheckmate((1, 0), Examples.ChessBoards[2]) == 'b');
/* 3*/allCases.Add(Chess.Validation.IsInCheckmate((1, 0), Examples.ChessBoards[3]) == 'n');
/* 4*/allCases.Add(Chess.Validation.IsInCheckmate((1, 0), Examples.ChessBoards[4]) == 'c');
/* 5*/allCases.Add(Chess.Validation.IsInCheckmate((2, 0), Examples.ChessBoards[5]) == 'n');
/* 6*/allCases.Add(Chess.Validation.IsInStalemate((2, 2), Examples.ChessBoards[6]));
/* 7*/allCases.Add(Chess.Validation.AllCheck((3, 3), Examples.ChessBoards[7]).Count == 6);
/* 8*/allCases.Add(Chess.Validation.AllCheck((2, 2), Examples.ChessBoards[8]).Count == 0);
			Chess.Chess.ChessBoard = Examples.ChessBoards[9];
/* 9*/allCases.Add(Chess.Utils.GetHowMove((0, 3), (1, 2)) == Chess.EnumMoves.EnPassant);
/*10*/allCases.Add(Chess.Utils.GetHowMove((4, 4), (3, 5)) == Chess.EnumMoves.EnPassant);
			Chess.Chess.ChessBoard = Examples.ChessBoards[10];
/*11*/allCases.Add(Chess.Utils.GetHowMove((2, 1), (2, 0)) == Chess.EnumMoves.Promotion);
/*12*/allCases.Add(Chess.Utils.GetHowMove((4, 6), (4, 7)) == Chess.EnumMoves.Promotion);
/*13*/allCases.Add(Chess.Utils.KingFinder(true, Examples.ChessBoards[11]) == (7, 7));
/*14*/allCases.Add(Chess.Utils.KingFinder(false, Examples.ChessBoards[11]) == (0, 0));
/*15*/allCases.Add(Chess.Utils.KingFinder(true, Examples.ChessBoards[12]) == (-1, -1));
/*16*/allCases.Add(Chess.Utils.KingFinder(false, Examples.ChessBoards[12]) == (-1, -1));
/*17*/allCases.Add(CompareArrays(Chess.Chess.ChessAging(Examples.ChessBoards[13]), Examples.ChessBoards[14]));
			return allCases;
		}

		public static bool CompareArrays (char[,] array1, char[,] array2)
		{
			return array1.Rank == array2.Rank && Enumerable.Range(0, array1.Rank).All(dimension => array1.GetLength(dimension) == array2.GetLength(dimension)) && array1.Cast<char>().SequenceEqual(array2.Cast<char>());
		}
	}
}