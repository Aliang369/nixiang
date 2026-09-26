using System;

[Serializable]
public class zbChapterOneBarTargetCsvTableData
{
	public zbChapterOneBarTargetCsvRecord[] records;

	public static zbChapterOneBarTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterOneBarTargetCsvTableData zbChapterOneBarTargetCsvTableData2 = new zbChapterOneBarTargetCsvTableData();
		zbChapterOneBarTargetCsvRecord[] array = new zbChapterOneBarTargetCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterOneBarTargetCsvRecord.Read(zbBinReader2);
		}
		zbChapterOneBarTargetCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterOneBarTargetCsvTableData2;
	}
}
