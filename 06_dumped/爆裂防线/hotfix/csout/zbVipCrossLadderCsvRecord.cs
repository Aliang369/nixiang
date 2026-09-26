using System;

[Serializable]
public class zbVipCrossLadderCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public string desc { get; set; }

	public int val { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbVipCrossLadderCsvRecord Read(zbBinReader r)
	{
		return new zbVipCrossLadderCsvRecord
		{
			type = r.ReadInt(),
			desc = r.ReadString(),
			val = r.ReadInt()
		};
	}
}
