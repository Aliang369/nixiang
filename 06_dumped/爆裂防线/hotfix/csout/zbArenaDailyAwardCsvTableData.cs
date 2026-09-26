using System;

[Serializable]
public class zbArenaDailyAwardCsvTableData
{
	public zbArenaDailyAwardCsvRecord[] records;

	public static zbArenaDailyAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbArenaDailyAwardCsvTableData zbArenaDailyAwardCsvTableData2 = new zbArenaDailyAwardCsvTableData();
		zbArenaDailyAwardCsvRecord[] array = new zbArenaDailyAwardCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbArenaDailyAwardCsvRecord.Read(zbBinReader2);
		}
		zbArenaDailyAwardCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbArenaDailyAwardCsvTableData2;
	}
}
