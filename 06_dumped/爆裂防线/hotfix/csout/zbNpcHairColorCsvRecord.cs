using System;

[Serializable]
public class zbNpcHairColorCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int[] color { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbNpcHairColorCsvRecord Read(zbBinReader r)
	{
		return new zbNpcHairColorCsvRecord
		{
			id = r.ReadInt(),
			color = r.ReadIntArray()
		};
	}
}
