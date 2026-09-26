using System;

[Serializable]
public class zbBossSubjectCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int sceneId { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int boss { get; set; }

	public int suitAvatarId { get; set; }

	public int gunId { get; set; }

	public int[][] zhanLingAward { get; set; }

	public int[] zhanLingPreView { get; set; }

	public int[] grandPrizeAward { get; set; }

	public string banerIcon { get; set; }

	public string skillIcon1 { get; set; }

	public string skillIcon2 { get; set; }

	public string skillIcon3 { get; set; }

	public string skilldesc1 { get; set; }

	public string skilldesc2 { get; set; }

	public string skilldesc3 { get; set; }

	public string desc { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbBossSubjectCsvRecord Read(zbBinReader r)
	{
		return new zbBossSubjectCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			sceneId = r.ReadInt(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			boss = r.ReadInt(),
			suitAvatarId = r.ReadInt(),
			gunId = r.ReadInt(),
			zhanLingAward = r.ReadIntArray2(),
			zhanLingPreView = r.ReadIntArray(),
			grandPrizeAward = r.ReadIntArray(),
			banerIcon = r.ReadString(),
			skillIcon1 = r.ReadString(),
			skillIcon2 = r.ReadString(),
			skillIcon3 = r.ReadString(),
			skilldesc1 = r.ReadString(),
			skilldesc2 = r.ReadString(),
			skilldesc3 = r.ReadString(),
			desc = r.ReadString()
		};
	}
}
