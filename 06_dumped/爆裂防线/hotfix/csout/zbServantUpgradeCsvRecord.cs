using System;

[Serializable]
public class zbServantUpgradeCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int[][] costList { get; set; }

	public int heroLevel { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbServantUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbServantUpgradeCsvRecord
		{
			lv = r.ReadInt(),
			costList = r.ReadIntArray2(),
			heroLevel = r.ReadInt()
		};
	}
}
