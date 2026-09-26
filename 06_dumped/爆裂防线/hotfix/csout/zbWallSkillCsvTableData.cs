using System;
using System.Collections.Generic;

[Serializable]
public class zbWallSkillCsvTableData
{
	public Dictionary<int, zbWallSkillCsvRecord> records = new Dictionary<int, zbWallSkillCsvRecord>();

	public static zbWallSkillCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWallSkillCsvTableData zbWallSkillCsvTableData2 = new zbWallSkillCsvTableData();
		Dictionary<int, zbWallSkillCsvRecord> dictionary = new Dictionary<int, zbWallSkillCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWallSkillCsvRecord.Read(zbBinReader2);
		}
		zbWallSkillCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWallSkillCsvTableData2;
	}
}
