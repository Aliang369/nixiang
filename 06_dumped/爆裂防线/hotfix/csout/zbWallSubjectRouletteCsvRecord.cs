using System;

[Serializable]
public class zbWallSubjectRouletteCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int id { get; set; }

	public int count { get; set; }

	public int rare { get; set; }

	public int rate { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWallSubjectRouletteCsvRecord Read(zbBinReader r)
	{
		return new zbWallSubjectRouletteCsvRecord
		{
			type = r.ReadInt(),
			id = r.ReadInt(),
			count = r.ReadInt(),
			rare = r.ReadInt(),
			rate = r.ReadInt()
		};
	}
}
