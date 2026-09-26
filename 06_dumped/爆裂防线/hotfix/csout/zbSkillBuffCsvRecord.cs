using System;

[Serializable]
public class zbSkillBuffCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string desc { get; set; }

	public string desc_zh { get; set; }

	public int buffIndex { get; set; }

	public float atkValRate { get; set; }

	public float[][] baseVal { get; set; }

	public int[] fatherVal { get; set; }

	public float fatherValRate { get; set; }

	public int attackType { get; set; }

	public string spinePath { get; set; }

	public int spinePoint { get; set; }

	public float param { get; set; }

	public int layer { get; set; }

	public int bossImmune { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSkillBuffCsvRecord Read(zbBinReader r)
	{
		return new zbSkillBuffCsvRecord
		{
			id = r.ReadInt(),
			desc = r.ReadString(),
			desc_zh = r.ReadString(),
			buffIndex = r.ReadInt(),
			atkValRate = r.ReadFloat(),
			baseVal = r.ReadFloatArray2(),
			fatherVal = r.ReadIntArray(),
			fatherValRate = r.ReadFloat(),
			attackType = r.ReadInt(),
			spinePath = r.ReadString(),
			spinePoint = r.ReadInt(),
			param = r.ReadFloat(),
			layer = r.ReadInt(),
			bossImmune = r.ReadInt()
		};
	}
}
