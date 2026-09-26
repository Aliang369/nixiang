using System;
using System.Collections.Generic;

[Serializable]
public class zbFootholdCsvTableData
{
	public Dictionary<int, zbFootholdCsvRecord> records = new Dictionary<int, zbFootholdCsvRecord>();

	public static zbFootholdCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFootholdCsvTableData zbFootholdCsvTableData2 = new zbFootholdCsvTableData();
		Dictionary<int, zbFootholdCsvRecord> dictionary = new Dictionary<int, zbFootholdCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFootholdCsvRecord.Read(zbBinReader2);
		}
		zbFootholdCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFootholdCsvTableData2;
	}
}
