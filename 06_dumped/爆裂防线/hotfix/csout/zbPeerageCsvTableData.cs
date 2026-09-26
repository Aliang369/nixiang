using System;
using System.Collections.Generic;

[Serializable]
public class zbPeerageCsvTableData
{
	public Dictionary<int, zbPeerageCsvRecord> records = new Dictionary<int, zbPeerageCsvRecord>();

	public static zbPeerageCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPeerageCsvTableData zbPeerageCsvTableData2 = new zbPeerageCsvTableData();
		Dictionary<int, zbPeerageCsvRecord> dictionary = new Dictionary<int, zbPeerageCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPeerageCsvRecord.Read(zbBinReader2);
		}
		zbPeerageCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPeerageCsvTableData2;
	}
}
