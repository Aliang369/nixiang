using System;

[Serializable]
public class zbLegionUnlockCsvRecord : zbTableBaseRecord
{
	public int func { get; set; }

	public string name { get; set; }

	public int legionLvl { get; set; }

	public int isShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionUnlockCsvRecord Read(zbBinReader r)
	{
		return new zbLegionUnlockCsvRecord
		{
			func = r.ReadInt(),
			name = r.ReadString(),
			legionLvl = r.ReadInt(),
			isShow = r.ReadInt()
		};
	}
}
