using System;
using System.Collections.Generic;

[Serializable]
public class zbLegionWarLevelCsvTableData
{
	public Dictionary<int, zbLegionWarLevelCsvRecord> records = new Dictionary<int, zbLegionWarLevelCsvRecord>();

	public static zbLegionWarLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionWarLevelCsvTableData zbLegionWarLevelCsvTableData2 = new zbLegionWarLevelCsvTableData();
		Dictionary<int, zbLegionWarLevelCsvRecord> dictionary = new Dictionary<int, zbLegionWarLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegionWarLevelCsvRecord.Read(zbBinReader2);
		}
		zbLegionWarLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegionWarLevelCsvTableData2;
	}
}
