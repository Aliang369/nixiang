using System;

[Serializable]
public class zbBarPoolNormalCsvTableData
{
	public zbBarPoolNormalCsvRecord[] records;

	public static zbBarPoolNormalCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBarPoolNormalCsvTableData zbBarPoolNormalCsvTableData2 = new zbBarPoolNormalCsvTableData();
		zbBarPoolNormalCsvRecord[] array = new zbBarPoolNormalCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbBarPoolNormalCsvRecord.Read(zbBinReader2);
		}
		zbBarPoolNormalCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbBarPoolNormalCsvTableData2;
	}
}
