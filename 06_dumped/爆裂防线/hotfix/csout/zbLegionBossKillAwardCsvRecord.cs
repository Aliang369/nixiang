using System;

[Serializable]
public class zbLegionBossKillAwardCsvRecord : zbTableBaseRecord
{
	public int bossid { get; set; }

	public int killCount { get; set; }

	public int pid { get; set; }

	public int[] awardShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionBossKillAwardCsvRecord Read(zbBinReader r)
	{
		return new zbLegionBossKillAwardCsvRecord
		{
			bossid = r.ReadInt(),
			killCount = r.ReadInt(),
			pid = r.ReadInt(),
			awardShow = r.ReadIntArray()
		};
	}
}
