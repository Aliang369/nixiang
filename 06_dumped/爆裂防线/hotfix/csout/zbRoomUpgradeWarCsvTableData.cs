using System;
using System.Collections.Generic;

[Serializable]
public class zbRoomUpgradeWarCsvTableData
{
	public Dictionary<int, zbRoomUpgradeWarCsvRecord> records = new Dictionary<int, zbRoomUpgradeWarCsvRecord>();

	public static zbRoomUpgradeWarCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbRoomUpgradeWarCsvTableData zbRoomUpgradeWarCsvTableData2 = new zbRoomUpgradeWarCsvTableData();
		Dictionary<int, zbRoomUpgradeWarCsvRecord> dictionary = new Dictionary<int, zbRoomUpgradeWarCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbRoomUpgradeWarCsvRecord.Read(zbBinReader2);
		}
		zbRoomUpgradeWarCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbRoomUpgradeWarCsvTableData2;
	}
}
