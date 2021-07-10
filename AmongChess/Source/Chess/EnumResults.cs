namespace AmongChess.Source.Chess
{
	internal enum EnumResults
	{
		DrawStalemate = 0,
		DrawMaterial,
		DrawFifty,
		DrawRepetition,
		DrawAgreement,
		DrawTimeout,
		WinCheckmate = 16,
		WinTimeout,
		WinResignation,
		MoveNormal = 32,
		ErrorInvalid = 48
	}
}
