using System;

[Serializable]
public class zbSeasonAirdropCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public string name { get; set; }

	public int totalProg { get; set; }

	public float forceSpeedFactor { get; set; }

	public int soloSpeed { get; set; }

	public int capacity { get; set; }

	public int[][] awardList { get; set; }

	public int winCount { get; set; }

	public int dailyLimit { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSeasonAirdropCsvRecord Read(zbBinReader r)
	{
		return new zbSeasonAirdropCsvRecord
		{
			quality = r.ReadInt(),
			name = r.ReadString(),
			totalProg = r.ReadInt(),
			forceSpeedFactor = r.ReadFloat(),
			soloSpeed = r.ReadInt(),
			capacity = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			winCount = r.ReadInt(),
			dailyLimit = r.ReadInt()
		};
	}
}
