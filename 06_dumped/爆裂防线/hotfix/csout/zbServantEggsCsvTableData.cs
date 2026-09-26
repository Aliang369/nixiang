using System;
using System.Collections.Generic;

[Serializable]
public class zbServantEggsCsvTableData
{
	public Dictionary<int, zbServantEggsCsvRecord> records = new Dictionary<int, zbServantEggsCsvRecord>();

	public static zbServantEggsCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbServantEggsCsvTableData zbServantEggsCsvTableData2 = new zbServantEggsCsvTableData();
		Dictionary<int, zbServantEggsCsvRecord> dictionary = new Dictionary<int, zbServantEggsCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbServantEggsCsvRecord.Read(zbBinReader2);
		}
		zbServantEggsCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbServantEggsCsvTableData2;
	}
}
