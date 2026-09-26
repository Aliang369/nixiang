using System;

[Serializable]
public class zbWorldBuildingCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string icon { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int easyDungeonId { get; set; }

	public int[] resOut { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldBuildingCsvRecord Read(zbBinReader r)
	{
		return new zbWorldBuildingCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			icon = r.ReadString(),
			name = r.ReadString(),
			desc = r.ReadString(),
			easyDungeonId = r.ReadInt(),
			resOut = r.ReadIntArray()
		};
	}
}
