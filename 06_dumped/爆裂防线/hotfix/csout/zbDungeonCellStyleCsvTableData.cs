using System;
using System.Collections.Generic;

[Serializable]
public class zbDungeonCellStyleCsvTableData
{
	public Dictionary<int, zbDungeonCellStyleCsvRecord> records = new Dictionary<int, zbDungeonCellStyleCsvRecord>();

	public static zbDungeonCellStyleCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbDungeonCellStyleCsvTableData zbDungeonCellStyleCsvTableData2 = new zbDungeonCellStyleCsvTableData();
		Dictionary<int, zbDungeonCellStyleCsvRecord> dictionary = new Dictionary<int, zbDungeonCellStyleCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbDungeonCellStyleCsvRecord.Read(zbBinReader2);
		}
		zbDungeonCellStyleCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbDungeonCellStyleCsvTableData2;
	}
}
