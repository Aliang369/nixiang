using System;

[Serializable]
public class zbMechaSubjectCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int mechaId { get; set; }

	public int[][] chapterGrandAward { get; set; }

	public int[][] zhanLingAward { get; set; }

	public int[] zhanLingPreView { get; set; }

	public int[] grandPrizeAward { get; set; }

	public string banerIcon { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMechaSubjectCsvRecord Read(zbBinReader r)
	{
		return new zbMechaSubjectCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			mechaId = r.ReadInt(),
			chapterGrandAward = r.ReadIntArray2(),
			zhanLingAward = r.ReadIntArray2(),
			zhanLingPreView = r.ReadIntArray(),
			grandPrizeAward = r.ReadIntArray(),
			banerIcon = r.ReadString()
		};
	}
}
