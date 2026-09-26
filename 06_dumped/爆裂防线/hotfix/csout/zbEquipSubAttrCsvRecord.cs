using System;

[Serializable]
public class zbEquipSubAttrCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbEquipSubAttrCsvRecord Read(zbBinReader r)
	{
		return new zbEquipSubAttrCsvRecord
		{
			id = r.ReadInt()
		};
	}
}
