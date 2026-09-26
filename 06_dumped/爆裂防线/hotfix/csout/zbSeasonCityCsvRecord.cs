using System;

[Serializable]
public class zbSeasonCityCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int level { get; set; }

	public int[][] produce { get; set; }

	public int[][] firstCapReward { get; set; }

	public string image { get; set; }

	public string spine { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSeasonCityCsvRecord Read(zbBinReader r)
	{
		return new zbSeasonCityCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			level = r.ReadInt(),
			produce = r.ReadIntArray2(),
			firstCapReward = r.ReadIntArray2(),
			image = r.ReadString(),
			spine = r.ReadString()
		};
	}
}
