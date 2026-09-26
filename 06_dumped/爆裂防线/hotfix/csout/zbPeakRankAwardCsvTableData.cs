using System;
using System.Collections.Generic;

[Serializable]
public class zbPeakRankAwardCsvTableData
{
	public Dictionary<int, zbPeakRankAwardCsvRecord> records = new Dictionary<int, zbPeakRankAwardCsvRecord>();

	public static zbPeakRankAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPeakRankAwardCsvTableData zbPeakRankAwardCsvTableData2 = new zbPeakRankAwardCsvTableData();
		Dictionary<int, zbPeakRankAwardCsvRecord> dictionary = new Dictionary<int, zbPeakRankAwardCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPeakRankAwardCsvRecord.Read(zbBinReader2);
		}
		zbPeakRankAwardCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPeakRankAwardCsvTableData2;
	}
}
