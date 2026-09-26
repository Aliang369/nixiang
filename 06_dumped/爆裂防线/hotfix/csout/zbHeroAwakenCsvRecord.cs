using System;

[Serializable]
public class zbHeroAwakenCsvRecord : zbTableBaseRecord
{
	public int skillType { get; set; }

	public int awakenLevel { get; set; }

	public int warRoomLevel { get; set; }

	public int upgradeCost { get; set; }

	public int advanceCost { get; set; }

	public int attrSkillType { get; set; }

	public float attrValue { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHeroAwakenCsvRecord Read(zbBinReader r)
	{
		return new zbHeroAwakenCsvRecord
		{
			skillType = r.ReadInt(),
			awakenLevel = r.ReadInt(),
			warRoomLevel = r.ReadInt(),
			upgradeCost = r.ReadInt(),
			advanceCost = r.ReadInt(),
			attrSkillType = r.ReadInt(),
			attrValue = r.ReadFloat()
		};
	}
}
