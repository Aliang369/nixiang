using System;

[Serializable]
public class zbPeakMatchCsvRecord : zbTableBaseRecord
{
	public int rank { get; set; }

	public int fightRange { get; set; }

	public int vipFightRange { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPeakMatchCsvRecord Read(zbBinReader r)
	{
		return new zbPeakMatchCsvRecord
		{
			rank = r.ReadInt(),
			fightRange = r.ReadInt(),
			vipFightRange = r.ReadInt()
		};
	}
}
