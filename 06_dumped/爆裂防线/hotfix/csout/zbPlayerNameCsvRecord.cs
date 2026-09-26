using System;

[Serializable]
public class zbPlayerNameCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string firstname { get; set; }

	public string lastname { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPlayerNameCsvRecord Read(zbBinReader r)
	{
		return new zbPlayerNameCsvRecord
		{
			id = r.ReadInt(),
			firstname = r.ReadString(),
			lastname = r.ReadString()
		};
	}
}
