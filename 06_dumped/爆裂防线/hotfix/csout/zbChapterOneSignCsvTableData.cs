using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterOneSignCsvTableData
{
	public Dictionary<int, zbChapterOneSignCsvRecord> records = new Dictionary<int, zbChapterOneSignCsvRecord>();

	public static zbChapterOneSignCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterOneSignCsvTableData zbChapterOneSignCsvTableData2 = new zbChapterOneSignCsvTableData();
		Dictionary<int, zbChapterOneSignCsvRecord> dictionary = new Dictionary<int, zbChapterOneSignCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterOneSignCsvRecord.Read(zbBinReader2);
		}
		zbChapterOneSignCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterOneSignCsvTableData2;
	}
}
