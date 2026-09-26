using System;
using System.Collections.Generic;

[Serializable]
public class zbLegionWarAwardCsvTableData
{
	public Dictionary<int, zbLegionWarAwardCsvRecord> records = new Dictionary<int, zbLegionWarAwardCsvRecord>();

	public static zbLegionWarAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionWarAwardCsvTableData zbLegionWarAwardCsvTableData2 = new zbLegionWarAwardCsvTableData();
		Dictionary<int, zbLegionWarAwardCsvRecord> dictionary = new Dictionary<int, zbLegionWarAwardCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegionWarAwardCsvRecord.Read(zbBinReader2);
		}
		zbLegionWarAwardCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegionWarAwardCsvTableData2;
	}
}
