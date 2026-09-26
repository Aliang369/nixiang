using System;

[Serializable]
public class zbModBattleRandCsvRecord : zbTableBaseRecord
{
	public int randId { get; set; }

	public int modId { get; set; }

	public int weight { get; set; }

	public int maxNum { get; set; }

	public int battleLv { get; set; }

	public int[] requireMod { get; set; }

	public int[] conflictMod { get; set; }

	public int[] requireSkillType { get; set; }

	public int[] conflictSkillId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbModBattleRandCsvRecord Read(zbBinReader r)
	{
		return new zbModBattleRandCsvRecord
		{
			randId = r.ReadInt(),
			modId = r.ReadInt(),
			weight = r.ReadInt(),
			maxNum = r.ReadInt(),
			battleLv = r.ReadInt(),
			requireMod = r.ReadIntArray(),
			conflictMod = r.ReadIntArray(),
			requireSkillType = r.ReadIntArray(),
			conflictSkillId = r.ReadIntArray()
		};
	}
}
