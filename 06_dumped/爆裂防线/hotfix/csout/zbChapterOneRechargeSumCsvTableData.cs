using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterOneRechargeSumCsvTableData
{
	public Dictionary<int, zbChapterOneRechargeSumCsvRecord> records = new Dictionary<int, zbChapterOneRechargeSumCsvRecord>();

	public static zbChapterOneRechargeSumCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterOneRechargeSumCsvTableData zbChapterOneRechargeSumCsvTableData2 = new zbChapterOneRechargeSumCsvTableData();
		Dictionary<int, zbChapterOneRechargeSumCsvRecord> dictionary = new Dictionary<int, zbChapterOneRechargeSumCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterOneRechargeSumCsvRecord.Read(zbBinReader2);
		}
		zbChapterOneRechargeSumCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterOneRechargeSumCsvTableData2;
	}
}
