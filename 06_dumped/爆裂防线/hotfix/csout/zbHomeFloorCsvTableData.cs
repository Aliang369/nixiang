using System;
using System.Collections.Generic;

[Serializable]
public class zbHomeFloorCsvTableData
{
	public Dictionary<int, zbHomeFloorCsvRecord> records = new Dictionary<int, zbHomeFloorCsvRecord>();

	public static zbHomeFloorCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHomeFloorCsvTableData zbHomeFloorCsvTableData2 = new zbHomeFloorCsvTableData();
		Dictionary<int, zbHomeFloorCsvRecord> dictionary = new Dictionary<int, zbHomeFloorCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHomeFloorCsvRecord.Read(zbBinReader2);
		}
		zbHomeFloorCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHomeFloorCsvTableData2;
	}
}
