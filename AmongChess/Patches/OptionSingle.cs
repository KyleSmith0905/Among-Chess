namespace AmongChess.Patches
{
	public class OptionSingle
	{
		public byte Id = 0;
		public string Name;
		public string[] AllValues;
		public byte Value = 0;
		public string Suffix = "";
		public (byte Id, byte[] Values) Exemption;
		public byte GroupId = byte.MaxValue;
	}

	public class OptionGroupSingle
	{
		public byte Id;
		public string Name;
		public string Value;
		public (byte OptionId, byte[] OptionValue, string ValueTo) Exemption;
	}
}
