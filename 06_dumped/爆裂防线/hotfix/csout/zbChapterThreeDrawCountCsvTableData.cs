using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterThreeDrawCountCsvTableData
{
	public Dictionary<int, zbChapterThreeDrawCountCsvRecord> records = new Dictionary<int, zbChapterThreeDrawCountCsvRecord>();

	public static zbChapterThreeDrawCountCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterThreeDrawCountCsvTableData zbChapterThreeDrawCountCsvTableData2 = new zbChapterThreeDrawCountCsvTableData();
		Dictionary<int, zbChapterThreeDrawCountCsvRecord> dictionary = new Dictionary<int, zbChapterThreeDrawCountCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterThreeDrawCountCsvRecord.Read(zbBinReader2);
		}
		zbChapterThreeDrawCountCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterThreeDrawCountCsvTableData2;
	}
}
