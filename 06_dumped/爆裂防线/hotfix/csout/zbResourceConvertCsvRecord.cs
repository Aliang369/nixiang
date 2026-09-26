using System;

[Serializable]
public class zbResourceConvertCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int resType { get; set; }

	public int resId { get; set; }

	public int resCount { get; set; }

	public int convertType { get; set; }

	public int convertId { get; set; }

	public int convertCount { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbResourceConvertCsvRecord Read(zbBinReader r)
	{
		return new zbResourceConvertCsvRecord
		{
			id = r.ReadInt(),
			resType = r.ReadInt(),
			resId = r.ReadInt(),
			resCount = r.ReadInt(),
			convertType = r.ReadInt(),
			convertId = r.ReadInt(),
			convertCount = r.ReadInt()
		};
	}
}
