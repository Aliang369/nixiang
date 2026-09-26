using System;
using System.Collections.Generic;

[Serializable]
public class zbDungeonCellTypeCsvTableData
{
	public Dictionary<int, zbDungeonCellTypeCsvRecord> records = new Dictionary<int, zbDungeonCellTypeCsvRecord>();

	public static zbDungeonCellTypeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbDungeonCellTypeCsvTableData zbDungeonCellTypeCsvTableData2 = new zbDungeonCellTypeCsvTableData();
		Dictionary<int, zbDungeonCellTypeCsvRecord> dictionary = new Dictionary<int, zbDungeonCellTypeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbDungeonCellTypeCsvRecord.Read(zbBinReader2);
		}
		zbDungeonCellTypeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbDungeonCellTypeCsvTableData2;
	}
}
