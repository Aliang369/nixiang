using System;

[Serializable]
public class zbWorldBossRankAwardCsvTableData
{
	public zbWorldBossRankAwardCsvRecord[] records;

	public static zbWorldBossRankAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldBossRankAwardCsvTableData zbWorldBossRankAwardCsvTableData2 = new zbWorldBossRankAwardCsvTableData();
		zbWorldBossRankAwardCsvRecord[] array = new zbWorldBossRankAwardCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbWorldBossRankAwardCsvRecord.Read(zbBinReader2);
		}
		zbWorldBossRankAwardCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbWorldBossRankAwardCsvTableData2;
	}
}
