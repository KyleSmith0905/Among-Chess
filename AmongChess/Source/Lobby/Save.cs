using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AmongChess.Source.Lobby
{
	internal class Save
	{
		public static void GameOptionsExport()
		{
			StringBuilder stringSettings = new StringBuilder();
			for (int i = 0; i < Options.AllOption.Count; i++)
			{
				_ = stringSettings.Append(Options.AllOption[i].Id.ToString());
				_ = stringSettings.Append(",");
				_ = stringSettings.Append(Options.AllOption[i].Value.ToString());
				_ = stringSettings.Append(";");
			}
			string path = Path.Combine(Application.persistentDataPath, "AmongChess-HostSettings");
			File.WriteAllText(path, stringSettings.ToString());
		}

		public static List<ClassOption> GameOptionsImport()
		{
			List<ClassOption> AllOptionsTemp = new List<ClassOption>();
			List<ClassOption> AllOptionsDefault = Options.OptionDefault();
			try
			{
				string path = Path.Combine(Application.persistentDataPath, "AmongChess-HostSettings");
				string text = File.ReadAllText(path);
				List<string> splitText = text.Split(";").ToList();
				List<byte> collectedId = new List<byte> { };
				for (int i = 0; i < splitText.Count; i++)
				{
					int splitter = splitText[i].IndexOf(",");
					if (splitter == -1) break;
					byte optionId = byte.Parse(splitText[i][0..splitter]);
					byte valueId = byte.Parse(splitText[i][(splitter + 1)..]);
					ClassOption optionSingle = AllOptionsDefault.Find(ele => ele.Id == optionId);
					if (valueId >= optionSingle.AllValues.Length) valueId = 0;
					if (optionSingle != null)
					{
						collectedId.Add(optionId);
						optionSingle.Id = optionId;
						optionSingle.Value = valueId;
						AllOptionsTemp.Add(optionSingle);
					}
				}
				for (int i = 0; i < AllOptionsDefault.Count; i++)
				{
					ClassOption optionSingle = AllOptionsDefault[i];
					if (!collectedId.Contains(optionSingle.Id)) AllOptionsTemp.Add(optionSingle);
				}
			}
			catch
			{
				AllOptionsTemp = AllOptionsDefault;
			}
			return AllOptionsTemp;
		}
	}
}
