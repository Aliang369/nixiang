using System;

[Serializable]
public class zbWallStarUpgradeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int star { get; set; }

	public int costProp { get; set; }

	public int damageImmunity { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWallStarUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbWallStarUpgradeCsvRecord
		{
			id = r.ReadInt(),
			star = r.ReadInt(),
			costProp = r.ReadInt(),
			damageImmunity = r.ReadInt()
		};
	}
}
