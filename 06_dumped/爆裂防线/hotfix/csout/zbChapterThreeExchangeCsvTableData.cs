using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterThreeExchangeCsvTableData
{
	public Dictionary<int, zbChapterThreeExchangeCsvRecord> records = new Dictionary<int, zbChapterThreeExchangeCsvRecord>();

	public static zbChapterThreeExchangeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterThreeExchangeCsvTableData zbChapterThreeExchangeCsvTableData2 = new zbChapterThreeExchangeCsvTableData();
		Dictionary<int, zbChapterThreeExchangeCsvRecord> dictionary = new Dictionary<int, zbChapterThreeExchangeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterThreeExchangeCsvRecord.Read(zbBinReader2);
		}
		zbChapterThreeExchangeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterThreeExchangeCsvTableData2;
	}
}
