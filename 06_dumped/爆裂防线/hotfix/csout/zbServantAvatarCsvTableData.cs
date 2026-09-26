using System;
using System.Collections.Generic;

[Serializable]
public class zbServantAvatarCsvTableData
{
	public Dictionary<int, zbServantAvatarCsvRecord> records = new Dictionary<int, zbServantAvatarCsvRecord>();

	public static zbServantAvatarCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbServantAvatarCsvTableData zbServantAvatarCsvTableData2 = new zbServantAvatarCsvTableData();
		Dictionary<int, zbServantAvatarCsvRecord> dictionary = new Dictionary<int, zbServantAvatarCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbServantAvatarCsvRecord.Read(zbBinReader2);
		}
		zbServantAvatarCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbServantAvatarCsvTableData2;
	}
}
