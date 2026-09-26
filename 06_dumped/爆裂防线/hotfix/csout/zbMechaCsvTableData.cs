using System;
using System.Collections.Generic;

[Serializable]
public class zbMechaCsvTableData
{
	public Dictionary<int, zbMechaCsvRecord> records = new Dictionary<int, zbMechaCsvRecord>();

	public static zbMechaCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaCsvTableData zbMechaCsvTableData2 = new zbMechaCsvTableData();
		Dictionary<int, zbMechaCsvRecord> dictionary = new Dictionary<int, zbMechaCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMechaCsvRecord.Read(zbBinReader2);
		}
		zbMechaCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMechaCsvTableData2;
	}
}
