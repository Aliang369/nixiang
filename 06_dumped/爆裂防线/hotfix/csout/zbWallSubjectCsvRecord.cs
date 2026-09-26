using System;

[Serializable]
public class zbWallSubjectCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int wallId { get; set; }

	public int[][] chapterGrandAward { get; set; }

	public int[][] zhanLingAward { get; set; }

	public int[] zhanLingPreView { get; set; }

	public int[] grandPrizeAward { get; set; }

	public string banerIcon { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWallSubjectCsvRecord Read(zbBinReader r)
	{
		return new zbWallSubjectCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			wallId = r.ReadInt(),
			chapterGrandAward = r.ReadIntArray2(),
			zhanLingAward = r.ReadIntArray2(),
			zhanLingPreView = r.ReadIntArray(),
			grandPrizeAward = r.ReadIntArray(),
			banerIcon = r.ReadString()
		};
	}
}
