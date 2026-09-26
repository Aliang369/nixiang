using System;

[Serializable]
public class zbVipFreeExpCsvRecord : zbTableBaseRecord
{
	public int day { get; set; }

	public int vipExp { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbVipFreeExpCsvRecord Read(zbBinReader r)
	{
		return new zbVipFreeExpCsvRecord
		{
			day = r.ReadInt(),
			vipExp = r.ReadInt()
		};
	}
}
