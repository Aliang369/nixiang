using System;
using System.Collections.Generic;

[Serializable]
public class zbSignTargetCsvTableData
{
	public Dictionary<int, zbSignTargetCsvRecord> records = new Dictionary<int, zbSignTargetCsvRecord>();

	public static zbSignTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSignTargetCsvTableData zbSignTargetCsvTableData2 = new zbSignTargetCsvTableData();
		Dictionary<int, zbSignTargetCsvRecord> dictionary = new Dictionary<int, zbSignTargetCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSignTargetCsvRecord.Read(zbBinReader2);
		}
		zbSignTargetCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSignTargetCsvTableData2;
	}
}
