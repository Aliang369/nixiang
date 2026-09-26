using System;

[Serializable]
public class zbMonsterAttrCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int valueType { get; set; }

	public int showPercent { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMonsterAttrCsvRecord Read(zbBinReader r)
	{
		return new zbMonsterAttrCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			valueType = r.ReadInt(),
			showPercent = r.ReadInt()
		};
	}
}
