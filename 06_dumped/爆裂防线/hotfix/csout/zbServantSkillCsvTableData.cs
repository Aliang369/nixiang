using System;
using System.Collections.Generic;

[Serializable]
public class zbServantSkillCsvTableData
{
	public Dictionary<int, zbServantSkillCsvRecord> records = new Dictionary<int, zbServantSkillCsvRecord>();

	public static zbServantSkillCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbServantSkillCsvTableData zbServantSkillCsvTableData2 = new zbServantSkillCsvTableData();
		Dictionary<int, zbServantSkillCsvRecord> dictionary = new Dictionary<int, zbServantSkillCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbServantSkillCsvRecord.Read(zbBinReader2);
		}
		zbServantSkillCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbServantSkillCsvTableData2;
	}
}
