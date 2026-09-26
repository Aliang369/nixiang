using System;
using System.Collections.Generic;

[Serializable]
public class zbFunctionUnlockCsvTableData
{
	public Dictionary<int, zbFunctionUnlockCsvRecord> records = new Dictionary<int, zbFunctionUnlockCsvRecord>();

	public static zbFunctionUnlockCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFunctionUnlockCsvTableData zbFunctionUnlockCsvTableData2 = new zbFunctionUnlockCsvTableData();
		Dictionary<int, zbFunctionUnlockCsvRecord> dictionary = new Dictionary<int, zbFunctionUnlockCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFunctionUnlockCsvRecord.Read(zbBinReader2);
		}
		zbFunctionUnlockCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFunctionUnlockCsvTableData2;
	}
}
