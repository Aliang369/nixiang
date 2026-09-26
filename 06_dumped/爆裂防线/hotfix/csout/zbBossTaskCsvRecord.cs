using System;

[Serializable]
public class zbBossTaskCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public int[][] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbBossTaskCsvRecord Read(zbBinReader r)
	{
		return new zbBossTaskCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			award = r.ReadIntArray2()
		};
	}
}
