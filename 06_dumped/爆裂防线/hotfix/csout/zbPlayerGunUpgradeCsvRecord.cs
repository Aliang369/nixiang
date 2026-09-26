using System;

[Serializable]
public class zbPlayerGunUpgradeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int star { get; set; }

	public int costProp { get; set; }

	public float dmg { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPlayerGunUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbPlayerGunUpgradeCsvRecord
		{
			id = r.ReadInt(),
			star = r.ReadInt(),
			costProp = r.ReadInt(),
			dmg = r.ReadFloat()
		};
	}
}
