using System;

[Serializable]
public class zbPlayerUpgradeCsvRecord : zbTableBaseRecord
{
	public int lvl { get; set; }

	public int exp { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPlayerUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbPlayerUpgradeCsvRecord
		{
			lvl = r.ReadInt(),
			exp = r.ReadInt()
		};
	}
}
