using System;

[Serializable]
public class zbBarPoolSpecialCsvRecord : zbTableBaseRecord
{
	public int poolType { get; set; }

	public int type { get; set; }

	public int id { get; set; }

	public int count { get; set; }

	public int weight { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbBarPoolSpecialCsvRecord Read(zbBinReader r)
	{
		return new zbBarPoolSpecialCsvRecord
		{
			poolType = r.ReadInt(),
			type = r.ReadInt(),
			id = r.ReadInt(),
			count = r.ReadInt(),
			weight = r.ReadInt()
		};
	}
}
