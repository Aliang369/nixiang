using System;

[Serializable]
public class zbLimitTimeRouletteCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int id { get; set; }

	public int count { get; set; }

	public int rate { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLimitTimeRouletteCsvRecord Read(zbBinReader r)
	{
		return new zbLimitTimeRouletteCsvRecord
		{
			type = r.ReadInt(),
			id = r.ReadInt(),
			count = r.ReadInt(),
			rate = r.ReadInt()
		};
	}
}
