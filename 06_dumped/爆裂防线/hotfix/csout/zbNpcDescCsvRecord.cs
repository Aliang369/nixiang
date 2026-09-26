using System;

[Serializable]
public class zbNpcDescCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string desc { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbNpcDescCsvRecord Read(zbBinReader r)
	{
		return new zbNpcDescCsvRecord
		{
			id = r.ReadInt(),
			desc = r.ReadString()
		};
	}
}
