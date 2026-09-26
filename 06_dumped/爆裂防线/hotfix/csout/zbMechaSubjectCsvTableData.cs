using System;
using System.Collections.Generic;

[Serializable]
public class zbMechaSubjectCsvTableData
{
	public Dictionary<int, zbMechaSubjectCsvRecord> records = new Dictionary<int, zbMechaSubjectCsvRecord>();

	public static zbMechaSubjectCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaSubjectCsvTableData zbMechaSubjectCsvTableData2 = new zbMechaSubjectCsvTableData();
		Dictionary<int, zbMechaSubjectCsvRecord> dictionary = new Dictionary<int, zbMechaSubjectCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMechaSubjectCsvRecord.Read(zbBinReader2);
		}
		zbMechaSubjectCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMechaSubjectCsvTableData2;
	}
}
