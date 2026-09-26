using System;

[Serializable]
public class zbGiftTaskCsvTableData
{
	public zbGiftTaskCsvRecord[] records;

	public static zbGiftTaskCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbGiftTaskCsvTableData zbGiftTaskCsvTableData2 = new zbGiftTaskCsvTableData();
		zbGiftTaskCsvRecord[] array = new zbGiftTaskCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbGiftTaskCsvRecord.Read(zbBinReader2);
		}
		zbGiftTaskCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbGiftTaskCsvTableData2;
	}
}
