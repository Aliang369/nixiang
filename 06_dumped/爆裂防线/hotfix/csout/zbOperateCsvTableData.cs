using System;
using System.Collections.Generic;

[Serializable]
public class zbOperateCsvTableData
{
	public Dictionary<int, zbOperateCsvRecord> records = new Dictionary<int, zbOperateCsvRecord>();

	public static zbOperateCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbOperateCsvTableData zbOperateCsvTableData2 = new zbOperateCsvTableData();
		Dictionary<int, zbOperateCsvRecord> dictionary = new Dictionary<int, zbOperateCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbOperateCsvRecord.Read(zbBinReader2);
		}
		zbOperateCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbOperateCsvTableData2;
	}
}
