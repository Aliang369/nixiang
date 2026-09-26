using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterThreeZhanLingCsvTableData
{
	public Dictionary<int, zbChapterThreeZhanLingCsvRecord> records = new Dictionary<int, zbChapterThreeZhanLingCsvRecord>();

	public static zbChapterThreeZhanLingCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterThreeZhanLingCsvTableData zbChapterThreeZhanLingCsvTableData2 = new zbChapterThreeZhanLingCsvTableData();
		Dictionary<int, zbChapterThreeZhanLingCsvRecord> dictionary = new Dictionary<int, zbChapterThreeZhanLingCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterThreeZhanLingCsvRecord.Read(zbBinReader2);
		}
		zbChapterThreeZhanLingCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterThreeZhanLingCsvTableData2;
	}
}
