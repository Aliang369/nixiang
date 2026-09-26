using System;
using System.Collections.Generic;

[Serializable]
public class zbPlayerGunCsvTableData
{
	public Dictionary<int, zbPlayerGunCsvRecord> records = new Dictionary<int, zbPlayerGunCsvRecord>();

	public static zbPlayerGunCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPlayerGunCsvTableData zbPlayerGunCsvTableData2 = new zbPlayerGunCsvTableData();
		Dictionary<int, zbPlayerGunCsvRecord> dictionary = new Dictionary<int, zbPlayerGunCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPlayerGunCsvRecord.Read(zbBinReader2);
		}
		zbPlayerGunCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPlayerGunCsvTableData2;
	}
}
