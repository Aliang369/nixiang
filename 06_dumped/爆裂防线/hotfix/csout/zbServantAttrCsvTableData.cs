using System;
using System.Collections.Generic;

[Serializable]
public class zbServantAttrCsvTableData
{
	public Dictionary<int, zbServantAttrCsvRecord> records = new Dictionary<int, zbServantAttrCsvRecord>();

	public static zbServantAttrCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbServantAttrCsvTableData zbServantAttrCsvTableData2 = new zbServantAttrCsvTableData();
		Dictionary<int, zbServantAttrCsvRecord> dictionary = new Dictionary<int, zbServantAttrCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbServantAttrCsvRecord.Read(zbBinReader2);
		}
		zbServantAttrCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbServantAttrCsvTableData2;
	}
}
