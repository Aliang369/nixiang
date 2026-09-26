using System;

[Serializable]
public class zbSignCsvRecord : zbTableBaseRecord
{
	public int day { get; set; }

	public int[] award { get; set; }

	public int doubleTag { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSignCsvRecord Read(zbBinReader r)
	{
		return new zbSignCsvRecord
		{
			day = r.ReadInt(),
			award = r.ReadIntArray(),
			doubleTag = r.ReadInt()
		};
	}
}
