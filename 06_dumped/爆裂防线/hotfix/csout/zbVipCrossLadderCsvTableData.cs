using System;
using System.Collections.Generic;

[Serializable]
public class zbVipCrossLadderCsvTableData
{
	public Dictionary<int, zbVipCrossLadderCsvRecord> records = new Dictionary<int, zbVipCrossLadderCsvRecord>();

	public static zbVipCrossLadderCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbVipCrossLadderCsvTableData zbVipCrossLadderCsvTableData2 = new zbVipCrossLadderCsvTableData();
		Dictionary<int, zbVipCrossLadderCsvRecord> dictionary = new Dictionary<int, zbVipCrossLadderCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbVipCrossLadderCsvRecord.Read(zbBinReader2);
		}
		zbVipCrossLadderCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbVipCrossLadderCsvTableData2;
	}
}
