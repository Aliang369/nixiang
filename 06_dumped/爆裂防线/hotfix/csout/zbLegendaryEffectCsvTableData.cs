using System;
using System.Collections.Generic;

[Serializable]
public class zbLegendaryEffectCsvTableData
{
	public Dictionary<int, zbLegendaryEffectCsvRecord> records = new Dictionary<int, zbLegendaryEffectCsvRecord>();

	public static zbLegendaryEffectCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegendaryEffectCsvTableData zbLegendaryEffectCsvTableData2 = new zbLegendaryEffectCsvTableData();
		Dictionary<int, zbLegendaryEffectCsvRecord> dictionary = new Dictionary<int, zbLegendaryEffectCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegendaryEffectCsvRecord.Read(zbBinReader2);
		}
		zbLegendaryEffectCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegendaryEffectCsvTableData2;
	}
}
