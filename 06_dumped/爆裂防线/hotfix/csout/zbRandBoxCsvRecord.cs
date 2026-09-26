using System;

[Serializable]
public class zbRandBoxCsvRecord : zbTableBaseRecord
{
	public int pid { get; set; }

	public int type { get; set; }

	public int id { get; set; }

	public int count { get; set; }

	public int weight { get; set; }

	public int preview { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbRandBoxCsvRecord Read(zbBinReader r)
	{
		return new zbRandBoxCsvRecord
		{
			pid = r.ReadInt(),
			type = r.ReadInt(),
			id = r.ReadInt(),
			count = r.ReadInt(),
			weight = r.ReadInt(),
			preview = r.ReadInt()
		};
	}
}
