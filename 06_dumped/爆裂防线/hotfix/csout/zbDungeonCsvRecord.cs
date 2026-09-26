using System;

[Serializable]
public class zbDungeonCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string icon { get; set; }

	public int size { get; set; }

	public int atkShow { get; set; }

	public int type { get; set; }

	public int sceneId { get; set; }

	public int costTili { get; set; }

	public int[][] awardList { get; set; }

	public int easyAwardRand { get; set; }

	public int riotAwardRand { get; set; }

	public int[][] bossAwardList { get; set; }

	public int bossAwardRand { get; set; }

	public int[][] awardShow { get; set; }

	public int[] npcAvatarId { get; set; }

	public int mstLv { get; set; }

	public int mstBoss { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbDungeonCsvRecord Read(zbBinReader r)
	{
		return new zbDungeonCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			icon = r.ReadString(),
			size = r.ReadInt(),
			atkShow = r.ReadInt(),
			type = r.ReadInt(),
			sceneId = r.ReadInt(),
			costTili = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			easyAwardRand = r.ReadInt(),
			riotAwardRand = r.ReadInt(),
			bossAwardList = r.ReadIntArray2(),
			bossAwardRand = r.ReadInt(),
			awardShow = r.ReadIntArray2(),
			npcAvatarId = r.ReadIntArray(),
			mstLv = r.ReadInt(),
			mstBoss = r.ReadInt()
		};
	}
}
