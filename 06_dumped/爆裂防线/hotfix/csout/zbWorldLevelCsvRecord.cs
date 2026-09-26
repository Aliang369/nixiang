using System;

[Serializable]
public class zbWorldLevelCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int num { get; set; }

	public int weight1 { get; set; }

	public int weight2 { get; set; }

	public int weight3 { get; set; }

	public int weight4 { get; set; }

	public int weight5 { get; set; }

	public int weight6 { get; set; }

	public int weight7 { get; set; }

	public int mstLv { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldLevelCsvRecord Read(zbBinReader r)
	{
		return new zbWorldLevelCsvRecord
		{
			lv = r.ReadInt(),
			num = r.ReadInt(),
			weight1 = r.ReadInt(),
			weight2 = r.ReadInt(),
			weight3 = r.ReadInt(),
			weight4 = r.ReadInt(),
			weight5 = r.ReadInt(),
			weight6 = r.ReadInt(),
			weight7 = r.ReadInt(),
			mstLv = r.ReadInt()
		};
	}
}
