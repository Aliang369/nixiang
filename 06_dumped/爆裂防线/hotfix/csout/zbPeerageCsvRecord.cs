using System;

[Serializable]
public class zbPeerageCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int colorQuality { get; set; }

	public int taskIcon { get; set; }

	public string icon { get; set; }

	public int sceneId { get; set; }

	public int[][] awardList { get; set; }

	public int ifMax { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPeerageCsvRecord Read(zbBinReader r)
	{
		return new zbPeerageCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			colorQuality = r.ReadInt(),
			taskIcon = r.ReadInt(),
			icon = r.ReadString(),
			sceneId = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			ifMax = r.ReadInt()
		};
	}
}
