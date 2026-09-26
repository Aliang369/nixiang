using System;
using System.Collections.Generic;

[Serializable]
public class zbEquipCsvTableData
{
	public Dictionary<int, zbEquipCsvRecord> records = new Dictionary<int, zbEquipCsvRecord>();

	public static zbEquipCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbEquipCsvTableData zbEquipCsvTableData2 = new zbEquipCsvTableData();
		Dictionary<int, zbEquipCsvRecord> dictionary = new Dictionary<int, zbEquipCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbEquipCsvRecord.Read(zbBinReader2);
		}
		zbEquipCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbEquipCsvTableData2;
	}
}
