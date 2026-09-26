using System;

[Serializable]
public class zbTeamAttrCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string name_zh { get; set; }

	public int showPercent { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTeamAttrCsvRecord Read(zbBinReader r)
	{
		return new zbTeamAttrCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			name_zh = r.ReadString(),
			showPercent = r.ReadInt()
		};
	}
}
