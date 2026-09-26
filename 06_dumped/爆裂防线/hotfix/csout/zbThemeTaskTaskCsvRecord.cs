using System;

[Serializable]
public class zbThemeTaskTaskCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int gid { get; set; }

	public int type { get; set; }

	public int target { get; set; }

	public int exp { get; set; }

	public int[] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbThemeTaskTaskCsvRecord Read(zbBinReader r)
	{
		return new zbThemeTaskTaskCsvRecord
		{
			id = r.ReadInt(),
			gid = r.ReadInt(),
			type = r.ReadInt(),
			target = r.ReadInt(),
			exp = r.ReadInt(),
			award = r.ReadIntArray()
		};
	}
}
