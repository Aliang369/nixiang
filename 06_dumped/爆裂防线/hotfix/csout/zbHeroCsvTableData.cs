using System;
using System.Collections.Generic;

[Serializable]
public class zbHeroCsvTableData
{
	public Dictionary<int, zbHeroCsvRecord> records = new Dictionary<int, zbHeroCsvRecord>();

	public static zbHeroCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHeroCsvTableData zbHeroCsvTableData2 = new zbHeroCsvTableData();
		Dictionary<int, zbHeroCsvRecord> dictionary = new Dictionary<int, zbHeroCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHeroCsvRecord.Read(zbBinReader2);
		}
		zbHeroCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHeroCsvTableData2;
	}
}
