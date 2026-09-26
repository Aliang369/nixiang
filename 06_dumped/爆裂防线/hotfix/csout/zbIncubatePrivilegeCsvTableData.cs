using System;
using System.Collections.Generic;

[Serializable]
public class zbIncubatePrivilegeCsvTableData
{
	public Dictionary<int, zbIncubatePrivilegeCsvRecord> records = new Dictionary<int, zbIncubatePrivilegeCsvRecord>();

	public static zbIncubatePrivilegeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbIncubatePrivilegeCsvTableData zbIncubatePrivilegeCsvTableData2 = new zbIncubatePrivilegeCsvTableData();
		Dictionary<int, zbIncubatePrivilegeCsvRecord> dictionary = new Dictionary<int, zbIncubatePrivilegeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbIncubatePrivilegeCsvRecord.Read(zbBinReader2);
		}
		zbIncubatePrivilegeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbIncubatePrivilegeCsvTableData2;
	}
}
