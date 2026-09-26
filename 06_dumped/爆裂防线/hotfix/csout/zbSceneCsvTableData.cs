using System;
using System.Collections.Generic;

[Serializable]
public class zbSceneCsvTableData
{
	public Dictionary<int, zbSceneCsvRecord> records = new Dictionary<int, zbSceneCsvRecord>();

	public static zbSceneCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSceneCsvTableData zbSceneCsvTableData2 = new zbSceneCsvTableData();
		Dictionary<int, zbSceneCsvRecord> dictionary = new Dictionary<int, zbSceneCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSceneCsvRecord.Read(zbBinReader2);
		}
		zbSceneCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSceneCsvTableData2;
	}
}
