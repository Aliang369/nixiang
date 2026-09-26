using System;

[Serializable]
public class zbLimitTimeTreasureCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int id { get; set; }

	public int count { get; set; }

	public int rate { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLimitTimeTreasureCsvRecord Read(zbBinReader r)
	{
		return new zbLimitTimeTreasureCsvRecord
		{
			type = r.ReadInt(),
			id = r.ReadInt(),
			count = r.ReadInt(),
			rate = r.ReadInt()
		};
	}
}
