using System;
using System.Collections.Generic;

[Serializable]
public class zbNpcAvatarCsvTableData
{
	public Dictionary<int, zbNpcAvatarCsvRecord> records = new Dictionary<int, zbNpcAvatarCsvRecord>();

	public static zbNpcAvatarCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbNpcAvatarCsvTableData zbNpcAvatarCsvTableData2 = new zbNpcAvatarCsvTableData();
		Dictionary<int, zbNpcAvatarCsvRecord> dictionary = new Dictionary<int, zbNpcAvatarCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbNpcAvatarCsvRecord.Read(zbBinReader2);
		}
		zbNpcAvatarCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbNpcAvatarCsvTableData2;
	}
}
