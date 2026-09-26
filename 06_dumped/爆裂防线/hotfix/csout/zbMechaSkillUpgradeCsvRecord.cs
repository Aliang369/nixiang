using System;

[Serializable]
public class zbMechaSkillUpgradeCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public int lv { get; set; }

	public int mechaLv { get; set; }

	public int costNum { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMechaSkillUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbMechaSkillUpgradeCsvRecord
		{
			quality = r.ReadInt(),
			lv = r.ReadInt(),
			mechaLv = r.ReadInt(),
			costNum = r.ReadInt()
		};
	}
}
