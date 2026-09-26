using System;
using System.Collections.Generic;

[Serializable]
public class zbDungeonEventCsvTableData
{
	public Dictionary<int, zbDungeonEventCsvRecord> records = new Dictionary<int, zbDungeonEventCsvRecord>();

	public static zbDungeonEventCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbDungeonEventCsvTableData zbDungeonEventCsvTableData2 = new zbDungeonEventCsvTableData();
		Dictionary<int, zbDungeonEventCsvRecord> dictionary = new Dictionary<int, zbDungeonEventCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbDungeonEventCsvRecord.Read(zbBinReader2);
		}
		zbDungeonEventCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbDungeonEventCsvTableData2;
	}
}
