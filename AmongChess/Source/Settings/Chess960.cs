using System;

namespace AmongChess.Source.Settings
{
	internal class Chess960
	{
		public static char[,] GetBoard()
		{
			char[,] board = new char[8, 8] {
				{ 'n', 'n', 'n', 'n', 'n', 'n', 'n', 'n' },
				{ 'f', 'f', 'f', 'f', 'f', 'f', 'f', 'f' },
				{ '1', '1', '1', '1', '1', '1', '1', '1' },
				{ '1', '1', '1', '1', '1', '1', '1', '1' },
				{ '1', '1', '1', '1', '1', '1', '1', '1' },
				{ '1', '1', '1', '1', '1', '1', '1', '1' },
				{ 'F', 'F', 'F', 'F', 'F', 'F', 'F', 'F' },
				{ 'N', 'N', 'N', 'N', 'N', 'N', 'N', 'N' }
			};
			char[] newRow = RandomRow();
			Buffer.BlockCopy(Array.ConvertAll(newRow, character => char.ToLower(character)), 0, board, 0, 16);
			Buffer.BlockCopy(newRow, 0, board, 112, 16);
			return board;
		}

		public static char[] RandomRow()
		{
			char[] boardRow = new char[8] { 'N', 'N', 'N', 'N', 'N', 'N', 'N', 'N' };
			int kingPosition = UnityEngine.Random.RandomRangeInt(1, 7);
			boardRow[kingPosition] = 'M';
			boardRow[UnityEngine.Random.RandomRangeInt(0, kingPosition)] = 'H';
			boardRow[UnityEngine.Random.RandomRangeInt(kingPosition + 1, 8)] = 'H';
			for (int j = 0; j < 3; j++)
			{
				while (true)
				{
					int position = j == 2 ? UnityEngine.Random.RandomRangeInt(0, 8) : (UnityEngine.Random.RandomRangeInt(0, 4) * 2) + j;
					if (char.ToUpper(boardRow[position]) == 'N')
					{
						boardRow[position] = j == 2 ? 'Q' : 'B';
						break;
					}
				}
			}
			return boardRow;
		}
	}
}
