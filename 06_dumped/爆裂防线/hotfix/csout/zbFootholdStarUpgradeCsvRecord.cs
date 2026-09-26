using System;

[Serializable]
public class zbFootholdStarUpgradeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int star { get; set; }

	public int costProp { get; set; }

	public float dmg { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFootholdStarUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbFootholdStarUpgradeCsvRecord
		{
			id = r.ReadInt(),
			star = r.ReadInt(),
			costProp = r.ReadInt(),
			dmg = r.ReadFloat()
		};
	}
}
