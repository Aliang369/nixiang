using System;
using System.Collections.Generic;

[Serializable]
public class zbPeakRobotCsvTableData
{
	public Dictionary<int, zbPeakRobotCsvRecord> records = new Dictionary<int, zbPeakRobotCsvRecord>();

	public static zbPeakRobotCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPeakRobotCsvTableData zbPeakRobotCsvTableData2 = new zbPeakRobotCsvTableData();
		Dictionary<int, zbPeakRobotCsvRecord> dictionary = new Dictionary<int, zbPeakRobotCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPeakRobotCsvRecord.Read(zbBinReader2);
		}
		zbPeakRobotCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPeakRobotCsvTableData2;
	}
}
