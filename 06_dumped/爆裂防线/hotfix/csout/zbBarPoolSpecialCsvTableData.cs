using System;

[Serializable]
public class zbBarPoolSpecialCsvTableData
{
	public zbBarPoolSpecialCsvRecord[] records;

	public static zbBarPoolSpecialCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBarPoolSpecialCsvTableData zbBarPoolSpecialCsvTableData2 = new zbBarPoolSpecialCsvTableData();
		zbBarPoolSpecialCsvRecord[] array = new zbBarPoolSpecialCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbBarPoolSpecialCsvRecord.Read(zbBinReader2);
		}
		zbBarPoolSpecialCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbBarPoolSpecialCsvTableData2;
	}
}
