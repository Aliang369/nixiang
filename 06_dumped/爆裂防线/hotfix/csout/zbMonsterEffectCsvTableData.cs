using System;
using System.Collections.Generic;

[Serializable]
public class zbMonsterEffectCsvTableData
{
	public Dictionary<int, zbMonsterEffectCsvRecord> records = new Dictionary<int, zbMonsterEffectCsvRecord>();

	public static zbMonsterEffectCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMonsterEffectCsvTableData zbMonsterEffectCsvTableData2 = new zbMonsterEffectCsvTableData();
		Dictionary<int, zbMonsterEffectCsvRecord> dictionary = new Dictionary<int, zbMonsterEffectCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMonsterEffectCsvRecord.Read(zbBinReader2);
		}
		zbMonsterEffectCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMonsterEffectCsvTableData2;
	}
}
