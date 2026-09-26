using System;
using System.Collections.Generic;

[Serializable]
public class zbWorldBuildingCsvTableData
{
	public Dictionary<int, zbWorldBuildingCsvRecord> records = new Dictionary<int, zbWorldBuildingCsvRecord>();

	public static zbWorldBuildingCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldBuildingCsvTableData zbWorldBuildingCsvTableData2 = new zbWorldBuildingCsvTableData();
		Dictionary<int, zbWorldBuildingCsvRecord> dictionary = new Dictionary<int, zbWorldBuildingCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWorldBuildingCsvRecord.Read(zbBinReader2);
		}
		zbWorldBuildingCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWorldBuildingCsvTableData2;
	}
}
