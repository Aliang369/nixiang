using System;

[Serializable]
public class zbMonsterSkillCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int skillIndex { get; set; }

	public string animationName { get; set; }

	public int effectId { get; set; }

	public float[] param { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMonsterSkillCsvRecord Read(zbBinReader r)
	{
		return new zbMonsterSkillCsvRecord
		{
			id = r.ReadInt(),
			skillIndex = r.ReadInt(),
			animationName = r.ReadString(),
			effectId = r.ReadInt(),
			param = r.ReadFloatArray()
		};
	}
}
