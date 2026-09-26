using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterThreeSignCsvTableData
{
	public Dictionary<int, zbChapterThreeSignCsvRecord> records = new Dictionary<int, zbChapterThreeSignCsvRecord>();

	public static zbChapterThreeSignCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterThreeSignCsvTableData zbChapterThreeSignCsvTableData2 = new zbChapterThreeSignCsvTableData();
		Dictionary<int, zbChapterThreeSignCsvRecord> dictionary = new Dictionary<int, zbChapterThreeSignCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterThreeSignCsvRecord.Read(zbBinReader2);
		}
		zbChapterThreeSignCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterThreeSignCsvTableData2;
	}
}
