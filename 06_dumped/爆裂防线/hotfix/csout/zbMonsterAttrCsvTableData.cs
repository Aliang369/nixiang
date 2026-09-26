using System;
using System.Collections.Generic;

[Serializable]
public class zbMonsterAttrCsvTableData
{
	public Dictionary<int, zbMonsterAttrCsvRecord> records = new Dictionary<int, zbMonsterAttrCsvRecord>();

	public static zbMonsterAttrCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMonsterAttrCsvTableData zbMonsterAttrCsvTableData2 = new zbMonsterAttrCsvTableData();
		Dictionary<int, zbMonsterAttrCsvRecord> dictionary = new Dictionary<int, zbMonsterAttrCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMonsterAttrCsvRecord.Read(zbBinReader2);
		}
		zbMonsterAttrCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMonsterAttrCsvTableData2;
	}
}
