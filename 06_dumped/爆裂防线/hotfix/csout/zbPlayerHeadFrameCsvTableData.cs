using System;
using System.Collections.Generic;

[Serializable]
public class zbPlayerHeadFrameCsvTableData
{
	public Dictionary<int, zbPlayerHeadFrameCsvRecord> records = new Dictionary<int, zbPlayerHeadFrameCsvRecord>();

	public static zbPlayerHeadFrameCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPlayerHeadFrameCsvTableData zbPlayerHeadFrameCsvTableData2 = new zbPlayerHeadFrameCsvTableData();
		Dictionary<int, zbPlayerHeadFrameCsvRecord> dictionary = new Dictionary<int, zbPlayerHeadFrameCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPlayerHeadFrameCsvRecord.Read(zbBinReader2);
		}
		zbPlayerHeadFrameCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPlayerHeadFrameCsvTableData2;
	}
}
