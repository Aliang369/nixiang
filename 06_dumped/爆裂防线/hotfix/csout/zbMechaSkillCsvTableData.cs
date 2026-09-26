using System;
using System.Collections.Generic;

[Serializable]
public class zbMechaSkillCsvTableData
{
	public Dictionary<int, zbMechaSkillCsvRecord> records = new Dictionary<int, zbMechaSkillCsvRecord>();

	public static zbMechaSkillCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaSkillCsvTableData zbMechaSkillCsvTableData2 = new zbMechaSkillCsvTableData();
		Dictionary<int, zbMechaSkillCsvRecord> dictionary = new Dictionary<int, zbMechaSkillCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMechaSkillCsvRecord.Read(zbBinReader2);
		}
		zbMechaSkillCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMechaSkillCsvTableData2;
	}
}
