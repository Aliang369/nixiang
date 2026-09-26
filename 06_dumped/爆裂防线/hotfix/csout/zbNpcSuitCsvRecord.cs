using System;

[Serializable]
public class zbNpcSuitCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string icon { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbNpcSuitCsvRecord Read(zbBinReader r)
	{
		return new zbNpcSuitCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			icon = r.ReadString()
		};
	}
}
