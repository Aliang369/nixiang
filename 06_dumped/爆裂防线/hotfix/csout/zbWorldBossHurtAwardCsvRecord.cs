using System;

[Serializable]
public class zbWorldBossHurtAwardCsvRecord : zbTableBaseRecord
{
	public int bossid { get; set; }

	public long hurt { get; set; }

	public int pid { get; set; }

	public int[] awardShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldBossHurtAwardCsvRecord Read(zbBinReader r)
	{
		return new zbWorldBossHurtAwardCsvRecord
		{
			bossid = r.ReadInt(),
			hurt = r.ReadLong(),
			pid = r.ReadInt(),
			awardShow = r.ReadIntArray()
		};
	}
}
