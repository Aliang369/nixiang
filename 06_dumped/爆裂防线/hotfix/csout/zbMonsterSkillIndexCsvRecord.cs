using System;

[Serializable]
public class zbMonsterSkillIndexCsvRecord : zbTableBaseRecord
{
	public int skillIndex { get; set; }

	public int stopTrigger { get; set; }

	public int spineShow { get; set; }

	public string desc { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMonsterSkillIndexCsvRecord Read(zbBinReader r)
	{
		return new zbMonsterSkillIndexCsvRecord
		{
			skillIndex = r.ReadInt(),
			stopTrigger = r.ReadInt(),
			spineShow = r.ReadInt(),
			desc = r.ReadString()
		};
	}
}
