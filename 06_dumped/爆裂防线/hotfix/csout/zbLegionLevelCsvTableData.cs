using System;
using System.Collections.Generic;

[Serializable]
public class zbLegionLevelCsvTableData
{
	public Dictionary<int, zbLegionLevelCsvRecord> records = new Dictionary<int, zbLegionLevelCsvRecord>();

	public static zbLegionLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionLevelCsvTableData zbLegionLevelCsvTableData2 = new zbLegionLevelCsvTableData();
		Dictionary<int, zbLegionLevelCsvRecord> dictionary = new Dictionary<int, zbLegionLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegionLevelCsvRecord.Read(zbBinReader2);
		}
		zbLegionLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegionLevelCsvTableData2;
	}
}
