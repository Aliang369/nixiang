using System;

[Serializable]
public class zbStrongerCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public int index { get; set; }

	public int funcType { get; set; }

	public string link { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbStrongerCsvRecord Read(zbBinReader r)
	{
		return new zbStrongerCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			index = r.ReadInt(),
			funcType = r.ReadInt(),
			link = r.ReadString()
		};
	}
}
