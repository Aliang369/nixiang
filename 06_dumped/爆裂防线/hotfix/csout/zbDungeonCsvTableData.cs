using System;
using System.Collections.Generic;

[Serializable]
public class zbDungeonCsvTableData
{
	public Dictionary<int, zbDungeonCsvRecord> records = new Dictionary<int, zbDungeonCsvRecord>();

	public static zbDungeonCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbDungeonCsvTableData zbDungeonCsvTableData2 = new zbDungeonCsvTableData();
		Dictionary<int, zbDungeonCsvRecord> dictionary = new Dictionary<int, zbDungeonCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbDungeonCsvRecord.Read(zbBinReader2);
		}
		zbDungeonCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbDungeonCsvTableData2;
	}
}
