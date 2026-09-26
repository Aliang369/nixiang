using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterOneZhanLingCsvTableData
{
	public Dictionary<int, zbChapterOneZhanLingCsvRecord> records = new Dictionary<int, zbChapterOneZhanLingCsvRecord>();

	public static zbChapterOneZhanLingCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterOneZhanLingCsvTableData zbChapterOneZhanLingCsvTableData2 = new zbChapterOneZhanLingCsvTableData();
		Dictionary<int, zbChapterOneZhanLingCsvRecord> dictionary = new Dictionary<int, zbChapterOneZhanLingCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterOneZhanLingCsvRecord.Read(zbBinReader2);
		}
		zbChapterOneZhanLingCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterOneZhanLingCsvTableData2;
	}
}
