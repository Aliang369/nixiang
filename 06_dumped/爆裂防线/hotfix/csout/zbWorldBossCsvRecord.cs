using System;

[Serializable]
public class zbWorldBossCsvRecord : zbTableBaseRecord
{
	public int bossid { get; set; }

	public string levelName { get; set; }

	public int mstLv { get; set; }

	public int openMinDay { get; set; }

	public int openMaxDay { get; set; }

	public int passHurt0 { get; set; }

	public int passHurt1 { get; set; }

	public int sceneId { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int boss { get; set; }

	public string icon { get; set; }

	public int[][] awardShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldBossCsvRecord Read(zbBinReader r)
	{
		return new zbWorldBossCsvRecord
		{
			bossid = r.ReadInt(),
			levelName = r.ReadString(),
			mstLv = r.ReadInt(),
			openMinDay = r.ReadInt(),
			openMaxDay = r.ReadInt(),
			passHurt0 = r.ReadInt(),
			passHurt1 = r.ReadInt(),
			sceneId = r.ReadInt(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			boss = r.ReadInt(),
			icon = r.ReadString(),
			awardShow = r.ReadIntArray2()
		};
	}
}
