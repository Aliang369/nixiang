using System;

[Serializable]
public class zbLegionBossChallengeCsvRecord : zbTableBaseRecord
{
	public int bossid { get; set; }

	public string name { get; set; }

	public int[][] awardList { get; set; }

	public int monsterKill { get; set; }

	public int sceneId { get; set; }

	public int mstLv { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int mst5 { get; set; }

	public int[][] awardShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionBossChallengeCsvRecord Read(zbBinReader r)
	{
		return new zbLegionBossChallengeCsvRecord
		{
			bossid = r.ReadInt(),
			name = r.ReadString(),
			awardList = r.ReadIntArray2(),
			monsterKill = r.ReadInt(),
			sceneId = r.ReadInt(),
			mstLv = r.ReadInt(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			mst5 = r.ReadInt(),
			awardShow = r.ReadIntArray2()
		};
	}
}
