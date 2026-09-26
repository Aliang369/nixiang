using System;

[Serializable]
public class zbChapterThreeBarProbabilityCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int id { get; set; }

	public int count { get; set; }

	public float probability { get; set; }

	public int bigPrize { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterThreeBarProbabilityCsvRecord Read(zbBinReader r)
	{
		return new zbChapterThreeBarProbabilityCsvRecord
		{
			type = r.ReadInt(),
			id = r.ReadInt(),
			count = r.ReadInt(),
			probability = r.ReadFloat(),
			bigPrize = r.ReadInt()
		};
	}
}
