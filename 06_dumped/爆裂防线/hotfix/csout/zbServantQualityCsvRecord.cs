using System;

[Serializable]
public class zbServantQualityCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public int lvLimit { get; set; }

	public int costNum { get; set; }

	public int propId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbServantQualityCsvRecord Read(zbBinReader r)
	{
		return new zbServantQualityCsvRecord
		{
			quality = r.ReadInt(),
			lvLimit = r.ReadInt(),
			costNum = r.ReadInt(),
			propId = r.ReadInt()
		};
	}
}
