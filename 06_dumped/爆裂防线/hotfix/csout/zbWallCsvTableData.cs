using System;
using System.Collections.Generic;

[Serializable]
public class zbWallCsvTableData
{
	public Dictionary<int, zbWallCsvRecord> records = new Dictionary<int, zbWallCsvRecord>();

	public static zbWallCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWallCsvTableData zbWallCsvTableData2 = new zbWallCsvTableData();
		Dictionary<int, zbWallCsvRecord> dictionary = new Dictionary<int, zbWallCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWallCsvRecord.Read(zbBinReader2);
		}
		zbWallCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWallCsvTableData2;
	}
}
