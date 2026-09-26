using System;

[Serializable]
public class zbMineNodeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string icon_1 { get; set; }

	public string icon_2 { get; set; }

	public string img { get; set; }

	public int layer { get; set; }

	public int[] pixel { get; set; }

	public int[] resOut { get; set; }

	public int[] awardFight { get; set; }

	public int[] crossAwardFight { get; set; }

	public int[] saoDangAward { get; set; }

	public int sceneId { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int mst5 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMineNodeCsvRecord Read(zbBinReader r)
	{
		return new zbMineNodeCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			icon_1 = r.ReadString(),
			icon_2 = r.ReadString(),
			img = r.ReadString(),
			layer = r.ReadInt(),
			pixel = r.ReadIntArray(),
			resOut = r.ReadIntArray(),
			awardFight = r.ReadIntArray(),
			crossAwardFight = r.ReadIntArray(),
			saoDangAward = r.ReadIntArray(),
			sceneId = r.ReadInt(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			mst5 = r.ReadInt()
		};
	}
}
