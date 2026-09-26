using System;
using System.Collections.Generic;

[Serializable]
public class zbMonsterAvatarCsvTableData
{
	public Dictionary<int, zbMonsterAvatarCsvRecord> records = new Dictionary<int, zbMonsterAvatarCsvRecord>();

	public static zbMonsterAvatarCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMonsterAvatarCsvTableData zbMonsterAvatarCsvTableData2 = new zbMonsterAvatarCsvTableData();
		Dictionary<int, zbMonsterAvatarCsvRecord> dictionary = new Dictionary<int, zbMonsterAvatarCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMonsterAvatarCsvRecord.Read(zbBinReader2);
		}
		zbMonsterAvatarCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMonsterAvatarCsvTableData2;
	}
}
