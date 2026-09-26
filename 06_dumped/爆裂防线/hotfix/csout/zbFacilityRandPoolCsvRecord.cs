using System;

[Serializable]
public class zbFacilityRandPoolCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int pid { get; set; }

	public int awardType { get; set; }

	public int awardId { get; set; }

	public int awardCount { get; set; }

	public int weight { get; set; }

	public int preview { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFacilityRandPoolCsvRecord Read(zbBinReader r)
	{
		return new zbFacilityRandPoolCsvRecord
		{
			id = r.ReadInt(),
			pid = r.ReadInt(),
			awardType = r.ReadInt(),
			awardId = r.ReadInt(),
			awardCount = r.ReadInt(),
			weight = r.ReadInt(),
			preview = r.ReadInt()
		};
	}
}
