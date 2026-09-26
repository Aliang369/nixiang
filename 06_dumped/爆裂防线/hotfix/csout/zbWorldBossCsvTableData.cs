using System;
using System.Collections.Generic;

[Serializable]
public class zbWorldBossCsvTableData
{
	public Dictionary<int, zbWorldBossCsvRecord> records = new Dictionary<int, zbWorldBossCsvRecord>();

	public static zbWorldBossCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldBossCsvTableData zbWorldBossCsvTableData2 = new zbWorldBossCsvTableData();
		Dictionary<int, zbWorldBossCsvRecord> dictionary = new Dictionary<int, zbWorldBossCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWorldBossCsvRecord.Read(zbBinReader2);
		}
		zbWorldBossCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWorldBossCsvTableData2;
	}
}
