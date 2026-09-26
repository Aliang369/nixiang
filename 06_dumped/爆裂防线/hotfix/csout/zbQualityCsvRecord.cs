using System;

[Serializable]
public class zbQualityCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public string name { get; set; }

	public string color { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbQualityCsvRecord Read(zbBinReader r)
	{
		return new zbQualityCsvRecord
		{
			quality = r.ReadInt(),
			name = r.ReadString(),
			color = r.ReadString()
		};
	}
}
