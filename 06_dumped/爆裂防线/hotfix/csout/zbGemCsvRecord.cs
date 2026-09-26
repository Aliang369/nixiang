using System;

[Serializable]
public class zbGemCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string name { get; set; }

	public string icon { get; set; }

	public int quality { get; set; }

	public int groupId { get; set; }

	public int mod { get; set; }

	public int[] skillTypeId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbGemCsvRecord Read(zbBinReader r)
	{
		return new zbGemCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			name = r.ReadString(),
			icon = r.ReadString(),
			quality = r.ReadInt(),
			groupId = r.ReadInt(),
			mod = r.ReadInt(),
			skillTypeId = r.ReadIntArray()
		};
	}
}
