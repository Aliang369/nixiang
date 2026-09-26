using System;
using System.Collections.Generic;

[Serializable]
public class zbServantSkillUpgradeCsvTableData
{
	public Dictionary<int, zbServantSkillUpgradeCsvRecord> records = new Dictionary<int, zbServantSkillUpgradeCsvRecord>();

	public static zbServantSkillUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbServantSkillUpgradeCsvTableData zbServantSkillUpgradeCsvTableData2 = new zbServantSkillUpgradeCsvTableData();
		Dictionary<int, zbServantSkillUpgradeCsvRecord> dictionary = new Dictionary<int, zbServantSkillUpgradeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbServantSkillUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbServantSkillUpgradeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbServantSkillUpgradeCsvTableData2;
	}
}
