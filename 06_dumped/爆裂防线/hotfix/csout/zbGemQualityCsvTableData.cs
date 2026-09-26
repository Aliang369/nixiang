using System;
using System.Collections.Generic;

[Serializable]
public class zbGemQualityCsvTableData
{
	public Dictionary<int, zbGemQualityCsvRecord> records = new Dictionary<int, zbGemQualityCsvRecord>();

	public static zbGemQualityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbGemQualityCsvTableData zbGemQualityCsvTableData2 = new zbGemQualityCsvTableData();
		Dictionary<int, zbGemQualityCsvRecord> dictionary = new Dictionary<int, zbGemQualityCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbGemQualityCsvRecord.Read(zbBinReader2);
		}
		zbGemQualityCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbGemQualityCsvTableData2;
	}
}
