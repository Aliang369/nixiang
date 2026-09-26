using System;

[Serializable]
public class zbMechaSkillCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string upgradeDesc { get; set; }

	public string icon { get; set; }

	public string spinePath { get; set; }

	public string playAction { get; set; }

	public string playAnimation { get; set; }

	public float[] param { get; set; }

	public float[][] buff { get; set; }

	public int atkType { get; set; }

	public float damageRate { get; set; }

	public float damageGrowth { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	public string tempParam3 { get; set; }

	internal static zbMechaSkillCsvRecord Read(zbBinReader r)
	{
		return new zbMechaSkillCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			upgradeDesc = r.ReadString(),
			icon = r.ReadString(),
			spinePath = r.ReadString(),
			playAction = r.ReadString(),
			playAnimation = r.ReadString(),
			param = r.ReadFloatArray(),
			buff = r.ReadFloatArray2(),
			atkType = r.ReadInt(),
			damageRate = r.ReadFloat(),
			damageGrowth = r.ReadFloat()
		};
	}
}
