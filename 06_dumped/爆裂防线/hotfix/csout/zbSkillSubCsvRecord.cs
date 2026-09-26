using System;

[Serializable]
public class zbSkillSubCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int subIndex { get; set; }

	public float atkValRate { get; set; }

	public int[] fatherVal { get; set; }

	public float fatherValRate { get; set; }

	public float[][] baseVal { get; set; }

	public float duration { get; set; }

	public float[][] skillBuff { get; set; }

	public int[][] skillSub { get; set; }

	public int attackType { get; set; }

	public int[] tag { get; set; }

	public float[] param { get; set; }

	public int speedChange { get; set; }

	public int bottomLayer { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSkillSubCsvRecord Read(zbBinReader r)
	{
		return new zbSkillSubCsvRecord
		{
			id = r.ReadInt(),
			subIndex = r.ReadInt(),
			atkValRate = r.ReadFloat(),
			fatherVal = r.ReadIntArray(),
			fatherValRate = r.ReadFloat(),
			baseVal = r.ReadFloatArray2(),
			duration = r.ReadFloat(),
			skillBuff = r.ReadFloatArray2(),
			skillSub = r.ReadIntArray2(),
			attackType = r.ReadInt(),
			tag = r.ReadIntArray(),
			param = r.ReadFloatArray(),
			speedChange = r.ReadInt(),
			bottomLayer = r.ReadInt()
		};
	}
}
