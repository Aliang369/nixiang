using System;
using System.Collections.Generic;

[Serializable]
public class zbModHandbookTargetCsvTableData
{
	public Dictionary<int, zbModHandbookTargetCsvRecord> records = new Dictionary<int, zbModHandbookTargetCsvRecord>();

	public static zbModHandbookTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbModHandbookTargetCsvTableData zbModHandbookTargetCsvTableData2 = new zbModHandbookTargetCsvTableData();
		Dictionary<int, zbModHandbookTargetCsvRecord> dictionary = new Dictionary<int, zbModHandbookTargetCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbModHandbookTargetCsvRecord.Read(zbBinReader2);
		}
		zbModHandbookTargetCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbModHandbookTargetCsvTableData2;
	}
}
