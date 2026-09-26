using System;
using System.Collections.Generic;

[Serializable]
public class zbTacticQualityCsvTableData
{
	public Dictionary<int, zbTacticQualityCsvRecord> records = new Dictionary<int, zbTacticQualityCsvRecord>();

	public static zbTacticQualityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTacticQualityCsvTableData zbTacticQualityCsvTableData2 = new zbTacticQualityCsvTableData();
		Dictionary<int, zbTacticQualityCsvRecord> dictionary = new Dictionary<int, zbTacticQualityCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbTacticQualityCsvRecord.Read(zbBinReader2);
		}
		zbTacticQualityCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbTacticQualityCsvTableData2;
	}
}
