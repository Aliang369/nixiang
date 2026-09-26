using System;

[Serializable]
public class zbSkillCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string name { get; set; }

	public float[][] buff { get; set; }

	public int attackType { get; set; }

	public int[] tag { get; set; }

	public string desc { get; set; }

	public string bulletSpineName { get; set; }

	public string icon { get; set; }

	public int mod { get; set; }

	public string riseEffectId { get; set; }

	public string flyEffectId { get; set; }

	public string targetEffectId { get; set; }

	public int speedChange { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSkillCsvRecord Read(zbBinReader r)
	{
		return new zbSkillCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			name = r.ReadString(),
			buff = r.ReadFloatArray2(),
			attackType = r.ReadInt(),
			tag = r.ReadIntArray(),
			desc = r.ReadString(),
			bulletSpineName = r.ReadString(),
			icon = r.ReadString(),
			mod = r.ReadInt(),
			riseEffectId = r.ReadString(),
			flyEffectId = r.ReadString(),
			targetEffectId = r.ReadString(),
			speedChange = r.ReadInt()
		};
	}
}
