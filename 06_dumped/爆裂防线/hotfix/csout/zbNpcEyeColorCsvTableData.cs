using System;
using System.Collections.Generic;

[Serializable]
public class zbNpcEyeColorCsvTableData
{
	public Dictionary<int, zbNpcEyeColorCsvRecord> records = new Dictionary<int, zbNpcEyeColorCsvRecord>();

	public static zbNpcEyeColorCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbNpcEyeColorCsvTableData zbNpcEyeColorCsvTableData2 = new zbNpcEyeColorCsvTableData();
		Dictionary<int, zbNpcEyeColorCsvRecord> dictionary = new Dictionary<int, zbNpcEyeColorCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbNpcEyeColorCsvRecord.Read(zbBinReader2);
		}
		zbNpcEyeColorCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbNpcEyeColorCsvTableData2;
	}
}
