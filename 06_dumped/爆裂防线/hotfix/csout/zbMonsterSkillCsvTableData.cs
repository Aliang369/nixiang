using System;
using System.Collections.Generic;

[Serializable]
public class zbMonsterSkillCsvTableData
{
	public Dictionary<int, zbMonsterSkillCsvRecord> records = new Dictionary<int, zbMonsterSkillCsvRecord>();

	public static zbMonsterSkillCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMonsterSkillCsvTableData zbMonsterSkillCsvTableData2 = new zbMonsterSkillCsvTableData();
		Dictionary<int, zbMonsterSkillCsvRecord> dictionary = new Dictionary<int, zbMonsterSkillCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMonsterSkillCsvRecord.Read(zbBinReader2);
		}
		zbMonsterSkillCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMonsterSkillCsvTableData2;
	}
}
