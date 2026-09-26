using System;
using System.Collections.Generic;

[Serializable]
public class zbTaskTypeCsvTableData
{
	public Dictionary<int, zbTaskTypeCsvRecord> records = new Dictionary<int, zbTaskTypeCsvRecord>();

	public static zbTaskTypeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTaskTypeCsvTableData zbTaskTypeCsvTableData2 = new zbTaskTypeCsvTableData();
		Dictionary<int, zbTaskTypeCsvRecord> dictionary = new Dictionary<int, zbTaskTypeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbTaskTypeCsvRecord.Read(zbBinReader2);
		}
		zbTaskTypeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbTaskTypeCsvTableData2;
	}
}
