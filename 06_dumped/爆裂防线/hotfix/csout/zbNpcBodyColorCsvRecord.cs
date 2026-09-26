using System;

[Serializable]
public class zbNpcBodyColorCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int[] color { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbNpcBodyColorCsvRecord Read(zbBinReader r)
	{
		return new zbNpcBodyColorCsvRecord
		{
			id = r.ReadInt(),
			color = r.ReadIntArray()
		};
	}
}
