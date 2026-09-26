using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterOneExchangeCsvTableData
{
	public Dictionary<int, zbChapterOneExchangeCsvRecord> records = new Dictionary<int, zbChapterOneExchangeCsvRecord>();

	public static zbChapterOneExchangeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterOneExchangeCsvTableData zbChapterOneExchangeCsvTableData2 = new zbChapterOneExchangeCsvTableData();
		Dictionary<int, zbChapterOneExchangeCsvRecord> dictionary = new Dictionary<int, zbChapterOneExchangeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterOneExchangeCsvRecord.Read(zbBinReader2);
		}
		zbChapterOneExchangeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterOneExchangeCsvTableData2;
	}
}
