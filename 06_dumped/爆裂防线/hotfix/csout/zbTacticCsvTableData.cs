using System;
using System.Collections.Generic;

[Serializable]
public class zbTacticCsvTableData
{
	public Dictionary<int, zbTacticCsvRecord> records = new Dictionary<int, zbTacticCsvRecord>();

	public static zbTacticCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTacticCsvTableData zbTacticCsvTableData2 = new zbTacticCsvTableData();
		Dictionary<int, zbTacticCsvRecord> dictionary = new Dictionary<int, zbTacticCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbTacticCsvRecord.Read(zbBinReader2);
		}
		zbTacticCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbTacticCsvTableData2;
	}
}
