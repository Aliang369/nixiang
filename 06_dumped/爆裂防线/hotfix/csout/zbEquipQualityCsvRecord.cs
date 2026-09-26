using System;

[Serializable]
public class zbEquipQualityCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public float rate { get; set; }

	public float damagePct { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbEquipQualityCsvRecord Read(zbBinReader r)
	{
		return new zbEquipQualityCsvRecord
		{
			quality = r.ReadInt(),
			rate = r.ReadFloat(),
			damagePct = r.ReadFloat()
		};
	}
}
