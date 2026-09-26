using System;

[Serializable]
public class zbPlayerHeadCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string icon { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int open { get; set; }

	public int sort { get; set; }

	public int quality { get; set; }

	public int propId { get; set; }

	public int exchange { get; set; }

	public string version { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPlayerHeadCsvRecord Read(zbBinReader r)
	{
		return new zbPlayerHeadCsvRecord
		{
			id = r.ReadInt(),
			icon = r.ReadString(),
			name = r.ReadString(),
			desc = r.ReadString(),
			open = r.ReadInt(),
			sort = r.ReadInt(),
			quality = r.ReadInt(),
			propId = r.ReadInt(),
			exchange = r.ReadInt(),
			version = r.ReadString()
		};
	}
}
