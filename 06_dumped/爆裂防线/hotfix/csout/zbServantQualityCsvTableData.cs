using System;
using System.Collections.Generic;

[Serializable]
public class zbServantQualityCsvTableData
{
	public Dictionary<int, zbServantQualityCsvRecord> records = new Dictionary<int, zbServantQualityCsvRecord>();

	public static zbServantQualityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbServantQualityCsvTableData zbServantQualityCsvTableData2 = new zbServantQualityCsvTableData();
		Dictionary<int, zbServantQualityCsvRecord> dictionary = new Dictionary<int, zbServantQualityCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbServantQualityCsvRecord.Read(zbBinReader2);
		}
		zbServantQualityCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbServantQualityCsvTableData2;
	}
}
