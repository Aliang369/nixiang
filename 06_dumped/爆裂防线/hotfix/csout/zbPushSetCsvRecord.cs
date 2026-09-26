using System;

[Serializable]
public class zbPushSetCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPushSetCsvRecord Read(zbBinReader r)
	{
		return new zbPushSetCsvRecord
		{
			type = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString()
		};
	}
}
