using System;
using System.Collections.Generic;

[Serializable]
public class zbGeneCsvTableData
{
	public Dictionary<int, zbGeneCsvRecord> records = new Dictionary<int, zbGeneCsvRecord>();

	public static zbGeneCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbGeneCsvTableData zbGeneCsvTableData2 = new zbGeneCsvTableData();
		Dictionary<int, zbGeneCsvRecord> dictionary = new Dictionary<int, zbGeneCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbGeneCsvRecord.Read(zbBinReader2);
		}
		zbGeneCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbGeneCsvTableData2;
	}
}
