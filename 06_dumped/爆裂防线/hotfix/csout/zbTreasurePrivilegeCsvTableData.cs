using System;
using System.Collections.Generic;

[Serializable]
public class zbTreasurePrivilegeCsvTableData
{
	public Dictionary<int, zbTreasurePrivilegeCsvRecord> records = new Dictionary<int, zbTreasurePrivilegeCsvRecord>();

	public static zbTreasurePrivilegeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTreasurePrivilegeCsvTableData zbTreasurePrivilegeCsvTableData2 = new zbTreasurePrivilegeCsvTableData();
		Dictionary<int, zbTreasurePrivilegeCsvRecord> dictionary = new Dictionary<int, zbTreasurePrivilegeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbTreasurePrivilegeCsvRecord.Read(zbBinReader2);
		}
		zbTreasurePrivilegeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbTreasurePrivilegeCsvTableData2;
	}
}
