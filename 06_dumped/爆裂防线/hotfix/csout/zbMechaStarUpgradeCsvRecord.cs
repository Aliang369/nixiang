using System;

[Serializable]
public class zbMechaStarUpgradeCsvRecord : zbTableBaseRecord
{
	public int star { get; set; }

	public int costPropNum { get; set; }

	public float damageRate { get; set; }

	public int attack { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMechaStarUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbMechaStarUpgradeCsvRecord
		{
			star = r.ReadInt(),
			costPropNum = r.ReadInt(),
			damageRate = r.ReadFloat(),
			attack = r.ReadInt()
		};
	}
}
