using System;

[Serializable]
public class zbWallSkillCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int wallIndex { get; set; }

	public int wallId { get; set; }

	public string desc { get; set; }

	public int atkType { get; set; }

	public float[] param { get; set; }

	public float[][] buff { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWallSkillCsvRecord Read(zbBinReader r)
	{
		return new zbWallSkillCsvRecord
		{
			id = r.ReadInt(),
			wallIndex = r.ReadInt(),
			wallId = r.ReadInt(),
			desc = r.ReadString(),
			atkType = r.ReadInt(),
			param = r.ReadFloatArray(),
			buff = r.ReadFloatArray2()
		};
	}
}
