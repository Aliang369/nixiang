using System;

[Serializable]
public class zbViptdescCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int id { get; set; }

	public int isnew { get; set; }

	public string desc { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbViptdescCsvRecord Read(zbBinReader r)
	{
		return new zbViptdescCsvRecord
		{
			level = r.ReadInt(),
			id = r.ReadInt(),
			isnew = r.ReadInt(),
			desc = r.ReadString()
		};
	}
}
