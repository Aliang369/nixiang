using System;
using System.Collections.Generic;

[Serializable]
public class zbChapterTwoDiamondCostCsvTableData
{
	public Dictionary<int, zbChapterTwoDiamondCostCsvRecord> records = new Dictionary<int, zbChapterTwoDiamondCostCsvRecord>();

	public static zbChapterTwoDiamondCostCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterTwoDiamondCostCsvTableData zbChapterTwoDiamondCostCsvTableData2 = new zbChapterTwoDiamondCostCsvTableData();
		Dictionary<int, zbChapterTwoDiamondCostCsvRecord> dictionary = new Dictionary<int, zbChapterTwoDiamondCostCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbChapterTwoDiamondCostCsvRecord.Read(zbBinReader2);
		}
		zbChapterTwoDiamondCostCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbChapterTwoDiamondCostCsvTableData2;
	}
}
