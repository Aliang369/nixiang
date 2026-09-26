using System;
using System.Collections.Generic;

[Serializable]
public class zbFootholdEffectCsvTableData
{
	public Dictionary<int, zbFootholdEffectCsvRecord> records = new Dictionary<int, zbFootholdEffectCsvRecord>();

	public static zbFootholdEffectCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFootholdEffectCsvTableData zbFootholdEffectCsvTableData2 = new zbFootholdEffectCsvTableData();
		Dictionary<int, zbFootholdEffectCsvRecord> dictionary = new Dictionary<int, zbFootholdEffectCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFootholdEffectCsvRecord.Read(zbBinReader2);
		}
		zbFootholdEffectCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFootholdEffectCsvTableData2;
	}
}
