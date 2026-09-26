using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterTwoExchangeCsvTableData
{
	public Dictionary<int, zbChapterTwoExchangeCsvRecord> records = new Dictionary<int, zbChapterTwoExchangeCsvRecord>();

	public static zbChapterTwoExchangeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterTwoExchangeCsvTableData zbChapterTwoExchangeCsvTableData2 = new zbChapterTwoExchangeCsvTableData();
		Dictionary<int, zbChapterTwoExchangeCsvRecord> dictionary = new Dictionary<int, zbChapterTwoExchangeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterTwoExchangeCsvRecord.Read(zbBinReader2);
		}
		zbChapterTwoExchangeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterTwoExchangeCsvTableData2;
	}
}
