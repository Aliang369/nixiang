using System;

[Serializable]
public class zbBossRouletteCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int id { get; set; }

	public int count { get; set; }

	public int rare { get; set; }

	public int showWeight { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbBossRouletteCsvRecord Read(zbBinReader r)
	{
		return new zbBossRouletteCsvRecord
		{
			type = r.ReadInt(),
			id = r.ReadInt(),
			count = r.ReadInt(),
			rare = r.ReadInt(),
			showWeight = r.ReadInt()
		};
	}
}
