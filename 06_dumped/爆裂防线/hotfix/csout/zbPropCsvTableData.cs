using System;
using System.Collections.Generic;

[Serializable]
public class zbPropCsvTableData
{
	public Dictionary<int, zbPropCsvRecord> records = new Dictionary<int, zbPropCsvRecord>();

	public static zbPropCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPropCsvTableData zbPropCsvTableData2 = new zbPropCsvTableData();
		Dictionary<int, zbPropCsvRecord> dictionary = new Dictionary<int, zbPropCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPropCsvRecord.Read(zbBinReader2);
		}
		zbPropCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPropCsvTableData2;
	}
}
