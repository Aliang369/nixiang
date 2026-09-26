using System;

[Serializable]
public class zbDefenseAttrCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string name_zh { get; set; }

	public int showPercent { get; set; }

	public int[] showWall { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbDefenseAttrCsvRecord Read(zbBinReader r)
	{
		return new zbDefenseAttrCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			name_zh = r.ReadString(),
			showPercent = r.ReadInt(),
			showWall = r.ReadIntArray()
		};
	}
}
