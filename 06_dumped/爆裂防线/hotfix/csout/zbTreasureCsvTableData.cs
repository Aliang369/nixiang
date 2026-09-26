using System;
using System.Collections.Generic;

[Serializable]
public class zbTreasureCsvTableData
{
	public Dictionary<int, zbTreasureCsvRecord> records = new Dictionary<int, zbTreasureCsvRecord>();

	public static zbTreasureCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTreasureCsvTableData zbTreasureCsvTableData2 = new zbTreasureCsvTableData();
		Dictionary<int, zbTreasureCsvRecord> dictionary = new Dictionary<int, zbTreasureCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbTreasureCsvRecord.Read(zbBinReader2);
		}
		zbTreasureCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbTreasureCsvTableData2;
	}
}
