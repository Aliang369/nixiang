using System;

[Serializable]
public class zbLegionLevelCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int exp { get; set; }

	public int member { get; set; }

	public int tili { get; set; }

	public int donateMax { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionLevelCsvRecord Read(zbBinReader r)
	{
		return new zbLegionLevelCsvRecord
		{
			level = r.ReadInt(),
			exp = r.ReadInt(),
			member = r.ReadInt(),
			tili = r.ReadInt(),
			donateMax = r.ReadInt()
		};
	}
}
