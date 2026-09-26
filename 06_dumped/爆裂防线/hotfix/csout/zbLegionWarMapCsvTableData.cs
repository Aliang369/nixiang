using System;
using System.Collections.Generic;

[Serializable]
public class zbLegionWarMapCsvTableData
{
	public Dictionary<int, zbLegionWarMapCsvRecord> records = new Dictionary<int, zbLegionWarMapCsvRecord>();

	public static zbLegionWarMapCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionWarMapCsvTableData zbLegionWarMapCsvTableData2 = new zbLegionWarMapCsvTableData();
		Dictionary<int, zbLegionWarMapCsvRecord> dictionary = new Dictionary<int, zbLegionWarMapCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegionWarMapCsvRecord.Read(zbBinReader2);
		}
		zbLegionWarMapCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegionWarMapCsvTableData2;
	}
}
