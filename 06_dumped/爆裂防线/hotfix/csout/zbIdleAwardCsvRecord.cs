using System;

[Serializable]
public class zbIdleAwardCsvRecord : zbTableBaseRecord
{
	public int mainLevel { get; set; }

	public int gold { get; set; }

	public int randCount { get; set; }

	public int randPid { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbIdleAwardCsvRecord Read(zbBinReader r)
	{
		return new zbIdleAwardCsvRecord
		{
			mainLevel = r.ReadInt(),
			gold = r.ReadInt(),
			randCount = r.ReadInt(),
			randPid = r.ReadInt()
		};
	}
}
