using System;

[Serializable]
public class zbNpcEyeColorCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int[] color { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbNpcEyeColorCsvRecord Read(zbBinReader r)
	{
		return new zbNpcEyeColorCsvRecord
		{
			id = r.ReadInt(),
			color = r.ReadIntArray()
		};
	}
}
