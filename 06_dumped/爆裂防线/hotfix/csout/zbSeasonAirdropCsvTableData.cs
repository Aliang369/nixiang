using System;
using System.Collections.Generic;

[Serializable]
public class zbSeasonAirdropCsvTableData
{
	public Dictionary<int, zbSeasonAirdropCsvRecord> records = new Dictionary<int, zbSeasonAirdropCsvRecord>();

	public static zbSeasonAirdropCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSeasonAirdropCsvTableData zbSeasonAirdropCsvTableData2 = new zbSeasonAirdropCsvTableData();
		Dictionary<int, zbSeasonAirdropCsvRecord> dictionary = new Dictionary<int, zbSeasonAirdropCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSeasonAirdropCsvRecord.Read(zbBinReader2);
		}
		zbSeasonAirdropCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSeasonAirdropCsvTableData2;
	}
}
