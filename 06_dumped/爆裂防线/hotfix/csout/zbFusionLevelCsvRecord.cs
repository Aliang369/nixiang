using System;

[Serializable]
public class zbFusionLevelCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int exp { get; set; }

	public int score { get; set; }

	public int filltime { get; set; }

	public string icon { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFusionLevelCsvRecord Read(zbBinReader r)
	{
		return new zbFusionLevelCsvRecord
		{
			lv = r.ReadInt(),
			exp = r.ReadInt(),
			score = r.ReadInt(),
			filltime = r.ReadInt(),
			icon = r.ReadString()
		};
	}
}
