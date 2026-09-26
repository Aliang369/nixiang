using System;

[Serializable]
public class zbGuideTalkCsvTableData
{
	public zbGuideTalkCsvRecord[] records;

	public static zbGuideTalkCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbGuideTalkCsvTableData zbGuideTalkCsvTableData2 = new zbGuideTalkCsvTableData();
		zbGuideTalkCsvRecord[] array = new zbGuideTalkCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbGuideTalkCsvRecord.Read(zbBinReader2);
		}
		zbGuideTalkCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbGuideTalkCsvTableData2;
	}
}
