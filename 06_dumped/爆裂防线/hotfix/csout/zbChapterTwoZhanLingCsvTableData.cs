using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterTwoZhanLingCsvTableData
{
	public Dictionary<int, zbChapterTwoZhanLingCsvRecord> records = new Dictionary<int, zbChapterTwoZhanLingCsvRecord>();

	public static zbChapterTwoZhanLingCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterTwoZhanLingCsvTableData zbChapterTwoZhanLingCsvTableData2 = new zbChapterTwoZhanLingCsvTableData();
		Dictionary<int, zbChapterTwoZhanLingCsvRecord> dictionary = new Dictionary<int, zbChapterTwoZhanLingCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterTwoZhanLingCsvRecord.Read(zbBinReader2);
		}
		zbChapterTwoZhanLingCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterTwoZhanLingCsvTableData2;
	}
}
