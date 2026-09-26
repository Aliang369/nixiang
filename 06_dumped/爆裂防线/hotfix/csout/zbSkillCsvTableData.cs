using System;
using System.Collections.Generic;

[Serializable]
public class zbSkillCsvTableData
{
	public Dictionary<int, zbSkillCsvRecord> records = new Dictionary<int, zbSkillCsvRecord>();

	public static zbSkillCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSkillCsvTableData zbSkillCsvTableData2 = new zbSkillCsvTableData();
		Dictionary<int, zbSkillCsvRecord> dictionary = new Dictionary<int, zbSkillCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSkillCsvRecord.Read(zbBinReader2);
		}
		zbSkillCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSkillCsvTableData2;
	}
}
