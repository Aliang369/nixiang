using System;
using System.Collections.Generic;

[Serializable]
public class zbLegionWarPlayerRankCsvTableData
{
	public Dictionary<int, zbLegionWarPlayerRankCsvRecord> records = new Dictionary<int, zbLegionWarPlayerRankCsvRecord>();

	public static zbLegionWarPlayerRankCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionWarPlayerRankCsvTableData zbLegionWarPlayerRankCsvTableData2 = new zbLegionWarPlayerRankCsvTableData();
		Dictionary<int, zbLegionWarPlayerRankCsvRecord> dictionary = new Dictionary<int, zbLegionWarPlayerRankCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegionWarPlayerRankCsvRecord.Read(zbBinReader2);
		}
		zbLegionWarPlayerRankCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegionWarPlayerRankCsvTableData2;
	}
}
