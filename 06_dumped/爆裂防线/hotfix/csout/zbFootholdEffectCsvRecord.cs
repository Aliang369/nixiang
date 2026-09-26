using System;

[Serializable]
public class zbFootholdEffectCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int skillIndex { get; set; }

	public string animationName { get; set; }

	public int effectId { get; set; }

	public int atkType { get; set; }

	public string desc { get; set; }

	public float[] param { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFootholdEffectCsvRecord Read(zbBinReader r)
	{
		return new zbFootholdEffectCsvRecord
		{
			id = r.ReadInt(),
			skillIndex = r.ReadInt(),
			animationName = r.ReadString(),
			effectId = r.ReadInt(),
			atkType = r.ReadInt(),
			desc = r.ReadString(),
			param = r.ReadFloatArray()
		};
	}
}
