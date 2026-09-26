using System;
using System.Collections.Generic;

[Serializable]
public class zbLegionUnlockCsvTableData
{
	public Dictionary<int, zbLegionUnlockCsvRecord> records = new Dictionary<int, zbLegionUnlockCsvRecord>();

	public static zbLegionUnlockCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionUnlockCsvTableData zbLegionUnlockCsvTableData2 = new zbLegionUnlockCsvTableData();
		Dictionary<int, zbLegionUnlockCsvRecord> dictionary = new Dictionary<int, zbLegionUnlockCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegionUnlockCsvRecord.Read(zbBinReader2);
		}
		zbLegionUnlockCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegionUnlockCsvTableData2;
	}
}
