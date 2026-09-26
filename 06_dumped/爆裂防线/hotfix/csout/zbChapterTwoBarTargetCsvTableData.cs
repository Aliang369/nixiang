using System;

[Serializable]
public class zbChapterTwoBarTargetCsvTableData
{
	public zbChapterTwoBarTargetCsvRecord[] records;

	public static zbChapterTwoBarTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterTwoBarTargetCsvTableData zbChapterTwoBarTargetCsvTableData2 = new zbChapterTwoBarTargetCsvTableData();
		zbChapterTwoBarTargetCsvRecord[] array = new zbChapterTwoBarTargetCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterTwoBarTargetCsvRecord.Read(zbBinReader2);
		}
		zbChapterTwoBarTargetCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterTwoBarTargetCsvTableData2;
	}
}
