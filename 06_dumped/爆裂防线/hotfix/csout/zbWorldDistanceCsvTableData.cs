using System;
using System.Collections.Generic;

[Serializable]
public class zbWorldDistanceCsvTableData
{
	public Dictionary<int, zbWorldDistanceCsvRecord> records = new Dictionary<int, zbWorldDistanceCsvRecord>();

	public static zbWorldDistanceCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldDistanceCsvTableData zbWorldDistanceCsvTableData2 = new zbWorldDistanceCsvTableData();
		Dictionary<int, zbWorldDistanceCsvRecord> dictionary = new Dictionary<int, zbWorldDistanceCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWorldDistanceCsvRecord.Read(zbBinReader2);
		}
		zbWorldDistanceCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWorldDistanceCsvTableData2;
	}
}
