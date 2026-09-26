using System;

[Serializable]
public class zbTacticStarUpgradeCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public int star { get; set; }

	public int costFrag { get; set; }

	public float probability { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTacticStarUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbTacticStarUpgradeCsvRecord
		{
			quality = r.ReadInt(),
			star = r.ReadInt(),
			costFrag = r.ReadInt(),
			probability = r.ReadFloat()
		};
	}
}
