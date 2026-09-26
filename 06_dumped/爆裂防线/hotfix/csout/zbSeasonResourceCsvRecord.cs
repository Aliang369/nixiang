using System;

[Serializable]
public class zbSeasonResourceCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int nodeType { get; set; }

	public int level { get; set; }

	public int[][] produce { get; set; }

	public string image { get; set; }

	public string spine { get; set; }

	public int pvpRobotId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSeasonResourceCsvRecord Read(zbBinReader r)
	{
		return new zbSeasonResourceCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			nodeType = r.ReadInt(),
			level = r.ReadInt(),
			produce = r.ReadIntArray2(),
			image = r.ReadString(),
			spine = r.ReadString(),
			pvpRobotId = r.ReadInt()
		};
	}
}
