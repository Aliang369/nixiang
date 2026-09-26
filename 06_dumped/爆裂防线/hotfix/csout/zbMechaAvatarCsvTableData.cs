using System;
using System.Collections.Generic;

[Serializable]
public class zbMechaAvatarCsvTableData
{
	public Dictionary<int, zbMechaAvatarCsvRecord> records = new Dictionary<int, zbMechaAvatarCsvRecord>();

	public static zbMechaAvatarCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaAvatarCsvTableData zbMechaAvatarCsvTableData2 = new zbMechaAvatarCsvTableData();
		Dictionary<int, zbMechaAvatarCsvRecord> dictionary = new Dictionary<int, zbMechaAvatarCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMechaAvatarCsvRecord.Read(zbBinReader2);
		}
		zbMechaAvatarCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMechaAvatarCsvTableData2;
	}
}
