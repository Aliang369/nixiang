using System;
using System.Collections.Generic;

[Serializable]
public class zbServantUpgradeCsvTableData
{
	public Dictionary<int, zbServantUpgradeCsvRecord> records = new Dictionary<int, zbServantUpgradeCsvRecord>();

	public static zbServantUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbServantUpgradeCsvTableData zbServantUpgradeCsvTableData2 = new zbServantUpgradeCsvTableData();
		Dictionary<int, zbServantUpgradeCsvRecord> dictionary = new Dictionary<int, zbServantUpgradeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbServantUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbServantUpgradeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbServantUpgradeCsvTableData2;
	}
}
