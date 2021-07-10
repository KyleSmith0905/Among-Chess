using System;

namespace AmongChess.Source.Settings
{
	class Transcendental
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
			Buffer.BlockCopy(Array.ConvertAll(Chess960.RandomRow(), character => char.ToLower(character)), 0, board, 0, 16);
			Buffer.BlockCopy(Chess960.RandomRow(), 0, board, 112, 16);
			return board;
		}
	}
}
