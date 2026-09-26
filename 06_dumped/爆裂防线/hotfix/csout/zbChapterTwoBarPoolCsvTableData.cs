using System;

[Serializable]
public class zbChapterTwoBarPoolCsvTableData
{
	public zbChapterTwoBarPoolCsvRecord[] records;

	public static zbChapterTwoBarPoolCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterTwoBarPoolCsvTableData zbChapterTwoBarPoolCsvTableData2 = new zbChapterTwoBarPoolCsvTableData();
		zbChapterTwoBarPoolCsvRecord[] array = new zbChapterTwoBarPoolCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterTwoBarPoolCsvRecord.Read(zbBinReader2);
		}
		zbChapterTwoBarPoolCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterTwoBarPoolCsvTableData2;
	}
}
