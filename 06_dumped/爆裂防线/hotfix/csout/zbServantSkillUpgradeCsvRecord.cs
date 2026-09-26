using System;

[Serializable]
public class zbServantSkillUpgradeCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int[][] costList { get; set; }

	public int lvLimit { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbServantSkillUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbServantSkillUpgradeCsvRecord
		{
			lv = r.ReadInt(),
			costList = r.ReadIntArray2(),
			lvLimit = r.ReadInt()
		};
	}
}
