using System;

[Serializable]
public class zbLangTermCsvRecord : zbTableBaseRecord
{
	public string name { get; set; }

	public string desc { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLangTermCsvRecord Read(zbBinReader r)
	{
		return new zbLangTermCsvRecord
		{
			name = r.ReadString(),
			desc = r.ReadString()
		};
	}
}
