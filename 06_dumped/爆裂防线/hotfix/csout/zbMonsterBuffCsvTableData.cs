using System;
using System.Collections.Generic;

[Serializable]
public class zbMonsterBuffCsvTableData
{
	public Dictionary<int, zbMonsterBuffCsvRecord> records = new Dictionary<int, zbMonsterBuffCsvRecord>();

	public static zbMonsterBuffCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMonsterBuffCsvTableData zbMonsterBuffCsvTableData2 = new zbMonsterBuffCsvTableData();
		Dictionary<int, zbMonsterBuffCsvRecord> dictionary = new Dictionary<int, zbMonsterBuffCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMonsterBuffCsvRecord.Read(zbBinReader2);
		}
		zbMonsterBuffCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMonsterBuffCsvTableData2;
	}
}
