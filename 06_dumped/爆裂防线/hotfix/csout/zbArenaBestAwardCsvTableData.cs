using System;

[Serializable]
public class zbArenaBestAwardCsvTableData
{
	public zbArenaBestAwardCsvRecord[] records;

	public static zbArenaBestAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbArenaBestAwardCsvTableData zbArenaBestAwardCsvTableData2 = new zbArenaBestAwardCsvTableData();
		zbArenaBestAwardCsvRecord[] array = new zbArenaBestAwardCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbArenaBestAwardCsvRecord.Read(zbBinReader2);
		}
		zbArenaBestAwardCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbArenaBestAwardCsvTableData2;
	}
}
