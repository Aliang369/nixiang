using System;
using System.Collections.Generic;

[Serializable]
public class zbLegionWarLegionRankCsvTableData
{
	public Dictionary<int, zbLegionWarLegionRankCsvRecord> records = new Dictionary<int, zbLegionWarLegionRankCsvRecord>();

	public static zbLegionWarLegionRankCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionWarLegionRankCsvTableData zbLegionWarLegionRankCsvTableData2 = new zbLegionWarLegionRankCsvTableData();
		Dictionary<int, zbLegionWarLegionRankCsvRecord> dictionary = new Dictionary<int, zbLegionWarLegionRankCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegionWarLegionRankCsvRecord.Read(zbBinReader2);
		}
		zbLegionWarLegionRankCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegionWarLegionRankCsvTableData2;
	}
}
