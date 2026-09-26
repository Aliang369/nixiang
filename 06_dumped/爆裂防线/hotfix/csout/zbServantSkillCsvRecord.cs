using System;

[Serializable]
public class zbServantSkillCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string icon { get; set; }

	public string icon1 { get; set; }

	public string desc { get; set; }

	public string desc_zh { get; set; }

	public int skillType { get; set; }

	public float baseVar { get; set; }

	public float growthVar { get; set; }

	public int idIndex { get; set; }

	public float[] param { get; set; }

	public int subSkillId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbServantSkillCsvRecord Read(zbBinReader r)
	{
		return new zbServantSkillCsvRecord
		{
			id = r.ReadInt(),
			icon = r.ReadString(),
			icon1 = r.ReadString(),
			desc = r.ReadString(),
			desc_zh = r.ReadString(),
			skillType = r.ReadInt(),
			baseVar = r.ReadFloat(),
			growthVar = r.ReadFloat(),
			idIndex = r.ReadInt(),
			param = r.ReadFloatArray(),
			subSkillId = r.ReadInt()
		};
	}
}
