using System;

[Serializable]
public class zbWorldEventFightCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int sceneId { get; set; }

	public int quality { get; set; }

	public int[][] awardList { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int mst5 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldEventFightCsvRecord Read(zbBinReader r)
	{
		return new zbWorldEventFightCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			sceneId = r.ReadInt(),
			quality = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			mst5 = r.ReadInt()
		};
	}
}
