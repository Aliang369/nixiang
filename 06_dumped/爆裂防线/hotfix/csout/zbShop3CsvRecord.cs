using System;

[Serializable]
public class zbShop3CsvRecord : zbTableBaseRecord
{
	public int index { get; set; }

	public int sort { get; set; }

	public int effect { get; set; }

	public int off { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbShop3CsvRecord Read(zbBinReader r)
	{
		return new zbShop3CsvRecord
		{
			index = r.ReadInt(),
			sort = r.ReadInt(),
			effect = r.ReadInt(),
			off = r.ReadInt()
		};
	}
}
