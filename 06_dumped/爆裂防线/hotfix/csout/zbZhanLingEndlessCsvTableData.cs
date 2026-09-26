using System;
using System.Collections.Generic;

[Serializable]
public class zbZhanLingEndlessCsvTableData
{
	public Dictionary<int, zbZhanLingEndlessCsvRecord> records = new Dictionary<int, zbZhanLingEndlessCsvRecord>();

	public static zbZhanLingEndlessCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbZhanLingEndlessCsvTableData zbZhanLingEndlessCsvTableData2 = new zbZhanLingEndlessCsvTableData();
		Dictionary<int, zbZhanLingEndlessCsvRecord> dictionary = new Dictionary<int, zbZhanLingEndlessCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbZhanLingEndlessCsvRecord.Read(zbBinReader2);
		}
		zbZhanLingEndlessCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbZhanLingEndlessCsvTableData2;
	}
}
