using System;
using System.Collections.Generic;

[Serializable]
public class zbHeroRarityCsvTableData
{
	public Dictionary<int, zbHeroRarityCsvRecord> records = new Dictionary<int, zbHeroRarityCsvRecord>();

	public static zbHeroRarityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHeroRarityCsvTableData zbHeroRarityCsvTableData2 = new zbHeroRarityCsvTableData();
		Dictionary<int, zbHeroRarityCsvRecord> dictionary = new Dictionary<int, zbHeroRarityCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHeroRarityCsvRecord.Read(zbBinReader2);
		}
		zbHeroRarityCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHeroRarityCsvTableData2;
	}
}
