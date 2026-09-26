using System;
using System.Collections.Generic;

[Serializable]
public class zbGetWayCsvTableData
{
	public Dictionary<int, zbGetWayCsvRecord> records = new Dictionary<int, zbGetWayCsvRecord>();

	public static zbGetWayCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbGetWayCsvTableData zbGetWayCsvTableData2 = new zbGetWayCsvTableData();
		Dictionary<int, zbGetWayCsvRecord> dictionary = new Dictionary<int, zbGetWayCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbGetWayCsvRecord.Read(zbBinReader2);
		}
		zbGetWayCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbGetWayCsvTableData2;
	}
}
