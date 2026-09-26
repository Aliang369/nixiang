using System;
using System.Collections.Generic;

[Serializable]
public class zbHeroAvatarCsvTableData
{
	public Dictionary<int, zbHeroAvatarCsvRecord> records = new Dictionary<int, zbHeroAvatarCsvRecord>();

	public static zbHeroAvatarCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHeroAvatarCsvTableData zbHeroAvatarCsvTableData2 = new zbHeroAvatarCsvTableData();
		Dictionary<int, zbHeroAvatarCsvRecord> dictionary = new Dictionary<int, zbHeroAvatarCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHeroAvatarCsvRecord.Read(zbBinReader2);
		}
		zbHeroAvatarCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHeroAvatarCsvTableData2;
	}
}
