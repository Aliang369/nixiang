using System;

[Serializable]
public class zbPlayerSuitUpgradeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int star { get; set; }

	public int costProp { get; set; }

	public int atk { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPlayerSuitUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbPlayerSuitUpgradeCsvRecord
		{
			id = r.ReadInt(),
			star = r.ReadInt(),
			costProp = r.ReadInt(),
			atk = r.ReadInt()
		};
	}
}
