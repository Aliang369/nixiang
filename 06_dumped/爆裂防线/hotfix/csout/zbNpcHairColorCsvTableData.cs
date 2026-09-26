using System;
using System.Collections.Generic;

[Serializable]
public class zbNpcHairColorCsvTableData
{
	public Dictionary<int, zbNpcHairColorCsvRecord> records = new Dictionary<int, zbNpcHairColorCsvRecord>();

	public static zbNpcHairColorCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbNpcHairColorCsvTableData zbNpcHairColorCsvTableData2 = new zbNpcHairColorCsvTableData();
		Dictionary<int, zbNpcHairColorCsvRecord> dictionary = new Dictionary<int, zbNpcHairColorCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbNpcHairColorCsvRecord.Read(zbBinReader2);
		}
		zbNpcHairColorCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbNpcHairColorCsvTableData2;
	}
}
