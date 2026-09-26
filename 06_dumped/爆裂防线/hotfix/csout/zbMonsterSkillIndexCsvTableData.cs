using System;
using System.Collections.Generic;

[Serializable]
public class zbMonsterSkillIndexCsvTableData
{
	public Dictionary<int, zbMonsterSkillIndexCsvRecord> records = new Dictionary<int, zbMonsterSkillIndexCsvRecord>();

	public static zbMonsterSkillIndexCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMonsterSkillIndexCsvTableData zbMonsterSkillIndexCsvTableData2 = new zbMonsterSkillIndexCsvTableData();
		Dictionary<int, zbMonsterSkillIndexCsvRecord> dictionary = new Dictionary<int, zbMonsterSkillIndexCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMonsterSkillIndexCsvRecord.Read(zbBinReader2);
		}
		zbMonsterSkillIndexCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMonsterSkillIndexCsvTableData2;
	}
}
