using System;

[Serializable]
public class zbChapterOneBarPoolCsvRecord : zbTableBaseRecord
{
	public int poolType { get; set; }

	public int type { get; set; }

	public int id { get; set; }

	public int count { get; set; }

	public int weight { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterOneBarPoolCsvRecord Read(zbBinReader r)
	{
		return new zbChapterOneBarPoolCsvRecord
		{
			poolType = r.ReadInt(),
			type = r.ReadInt(),
			id = r.ReadInt(),
			count = r.ReadInt(),
			weight = r.ReadInt()
		};
	}
}
