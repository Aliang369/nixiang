using System;
using System.Collections.Generic;

[Serializable]
public class zbMechaSubjectZhanLingCsvTableData
{
	public Dictionary<int, zbMechaSubjectZhanLingCsvRecord> records = new Dictionary<int, zbMechaSubjectZhanLingCsvRecord>();

	public static zbMechaSubjectZhanLingCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaSubjectZhanLingCsvTableData zbMechaSubjectZhanLingCsvTableData2 = new zbMechaSubjectZhanLingCsvTableData();
		Dictionary<int, zbMechaSubjectZhanLingCsvRecord> dictionary = new Dictionary<int, zbMechaSubjectZhanLingCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMechaSubjectZhanLingCsvRecord.Read(zbBinReader2);
		}
		zbMechaSubjectZhanLingCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMechaSubjectZhanLingCsvTableData2;
	}
}
