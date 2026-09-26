using System;
using System.Collections.Generic;

[Serializable]
public class zbDefenseAttrCsvTableData
{
	public Dictionary<int, zbDefenseAttrCsvRecord> records = new Dictionary<int, zbDefenseAttrCsvRecord>();

	public static zbDefenseAttrCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbDefenseAttrCsvTableData zbDefenseAttrCsvTableData2 = new zbDefenseAttrCsvTableData();
		Dictionary<int, zbDefenseAttrCsvRecord> dictionary = new Dictionary<int, zbDefenseAttrCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbDefenseAttrCsvRecord.Read(zbBinReader2);
		}
		zbDefenseAttrCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbDefenseAttrCsvTableData2;
	}
}
