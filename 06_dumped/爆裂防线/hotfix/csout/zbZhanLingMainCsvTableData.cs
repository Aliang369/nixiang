using System;
using System.Collections.Generic;

[Serializable]
public class zbZhanLingMainCsvTableData
{
	public Dictionary<int, zbZhanLingMainCsvRecord> records = new Dictionary<int, zbZhanLingMainCsvRecord>();

	public static zbZhanLingMainCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbZhanLingMainCsvTableData zbZhanLingMainCsvTableData2 = new zbZhanLingMainCsvTableData();
		Dictionary<int, zbZhanLingMainCsvRecord> dictionary = new Dictionary<int, zbZhanLingMainCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbZhanLingMainCsvRecord.Read(zbBinReader2);
		}
		zbZhanLingMainCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbZhanLingMainCsvTableData2;
	}
}
