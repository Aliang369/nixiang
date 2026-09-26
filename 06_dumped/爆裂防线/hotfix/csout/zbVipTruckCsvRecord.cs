using System;

[Serializable]
public class zbVipTruckCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public string desc { get; set; }

	public int val { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbVipTruckCsvRecord Read(zbBinReader r)
	{
		return new zbVipTruckCsvRecord
		{
			type = r.ReadInt(),
			desc = r.ReadString(),
			val = r.ReadInt()
		};
	}
}
