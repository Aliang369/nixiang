using System;

[Serializable]
public class zbRankAwardCsvTableData
{
	public zbRankAwardCsvRecord[] records;

	public static zbRankAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbRankAwardCsvTableData zbRankAwardCsvTableData2 = new zbRankAwardCsvTableData();
		zbRankAwardCsvRecord[] array = new zbRankAwardCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbRankAwardCsvRecord.Read(zbBinReader2);
		}
		zbRankAwardCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbRankAwardCsvTableData2;
	}
}
