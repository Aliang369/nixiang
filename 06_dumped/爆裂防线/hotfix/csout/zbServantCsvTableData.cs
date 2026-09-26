using System;
using System.Collections.Generic;

[Serializable]
public class zbServantCsvTableData
{
	public Dictionary<int, zbServantCsvRecord> records = new Dictionary<int, zbServantCsvRecord>();

	public static zbServantCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbServantCsvTableData zbServantCsvTableData2 = new zbServantCsvTableData();
		Dictionary<int, zbServantCsvRecord> dictionary = new Dictionary<int, zbServantCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbServantCsvRecord.Read(zbBinReader2);
		}
		zbServantCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbServantCsvTableData2;
	}
}
