using System;
using System.Collections.Generic;

[Serializable]
public class zbMonsterCsvTableData
{
	public Dictionary<int, zbMonsterCsvRecord> records = new Dictionary<int, zbMonsterCsvRecord>();

	public static zbMonsterCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMonsterCsvTableData zbMonsterCsvTableData2 = new zbMonsterCsvTableData();
		Dictionary<int, zbMonsterCsvRecord> dictionary = new Dictionary<int, zbMonsterCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMonsterCsvRecord.Read(zbBinReader2);
		}
		zbMonsterCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMonsterCsvTableData2;
	}
}
