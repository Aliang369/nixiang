using System;
using System.Collections.Generic;

[Serializable]
public class zbLadderCsvTableData
{
	public Dictionary<int, zbLadderCsvRecord> records = new Dictionary<int, zbLadderCsvRecord>();

	public static zbLadderCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLadderCsvTableData zbLadderCsvTableData2 = new zbLadderCsvTableData();
		Dictionary<int, zbLadderCsvRecord> dictionary = new Dictionary<int, zbLadderCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLadderCsvRecord.Read(zbBinReader2);
		}
		zbLadderCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLadderCsvTableData2;
	}
}
