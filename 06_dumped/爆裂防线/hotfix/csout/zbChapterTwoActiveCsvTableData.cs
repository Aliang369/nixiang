using System;

[Serializable]
public class zbChapterTwoActiveCsvTableData
{
	public zbChapterTwoActiveCsvRecord[] records;

	public static zbChapterTwoActiveCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterTwoActiveCsvTableData zbChapterTwoActiveCsvTableData2 = new zbChapterTwoActiveCsvTableData();
		zbChapterTwoActiveCsvRecord[] array = new zbChapterTwoActiveCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterTwoActiveCsvRecord.Read(zbBinReader2);
		}
		zbChapterTwoActiveCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterTwoActiveCsvTableData2;
	}
}
