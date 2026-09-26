using System;

[Serializable]
public class zbLadderCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string icon { get; set; }

	public string spine { get; set; }

	public int star { get; set; }

	public int quality { get; set; }

	public int score { get; set; }

	public int[][] settleAward { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int mst5 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLadderCsvRecord Read(zbBinReader r)
	{
		return new zbLadderCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			icon = r.ReadString(),
			spine = r.ReadString(),
			star = r.ReadInt(),
			quality = r.ReadInt(),
			score = r.ReadInt(),
			settleAward = r.ReadIntArray2(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			mst5 = r.ReadInt()
		};
	}
}
