using System;

[Serializable]
public class zbGemQualityCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public float critDamage { get; set; }

	public int[] getway { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbGemQualityCsvRecord Read(zbBinReader r)
	{
		return new zbGemQualityCsvRecord
		{
			quality = r.ReadInt(),
			critDamage = r.ReadFloat(),
			getway = r.ReadIntArray()
		};
	}
}
