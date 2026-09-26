using System;
using System.Collections.Generic;

[Serializable]
public class zbTrainCsvTableData
{
	public Dictionary<int, zbTrainCsvRecord> records = new Dictionary<int, zbTrainCsvRecord>();

	public static zbTrainCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTrainCsvTableData zbTrainCsvTableData2 = new zbTrainCsvTableData();
		Dictionary<int, zbTrainCsvRecord> dictionary = new Dictionary<int, zbTrainCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbTrainCsvRecord.Read(zbBinReader2);
		}
		zbTrainCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbTrainCsvTableData2;
	}
}
