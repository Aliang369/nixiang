using System;

[Serializable]
public class zbLimitTimeVendorCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int id { get; set; }

	public int count { get; set; }

	public int rate { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLimitTimeVendorCsvRecord Read(zbBinReader r)
	{
		return new zbLimitTimeVendorCsvRecord
		{
			type = r.ReadInt(),
			id = r.ReadInt(),
			count = r.ReadInt(),
			rate = r.ReadInt()
		};
	}
}
