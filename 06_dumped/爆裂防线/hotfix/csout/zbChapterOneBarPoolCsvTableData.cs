using System;

[Serializable]
public class zbChapterOneBarPoolCsvTableData
{
	public zbChapterOneBarPoolCsvRecord[] records;

	public static zbChapterOneBarPoolCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterOneBarPoolCsvTableData zbChapterOneBarPoolCsvTableData2 = new zbChapterOneBarPoolCsvTableData();
		zbChapterOneBarPoolCsvRecord[] array = new zbChapterOneBarPoolCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterOneBarPoolCsvRecord.Read(zbBinReader2);
		}
		zbChapterOneBarPoolCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterOneBarPoolCsvTableData2;
	}
}
