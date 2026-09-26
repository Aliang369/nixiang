using System;
using System.Collections.Generic;

[Serializable]
public class zbSkillTypeCsvTableData
{
	public Dictionary<int, zbSkillTypeCsvRecord> records = new Dictionary<int, zbSkillTypeCsvRecord>();

	public static zbSkillTypeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSkillTypeCsvTableData zbSkillTypeCsvTableData2 = new zbSkillTypeCsvTableData();
		Dictionary<int, zbSkillTypeCsvRecord> dictionary = new Dictionary<int, zbSkillTypeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSkillTypeCsvRecord.Read(zbBinReader2);
		}
		zbSkillTypeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSkillTypeCsvTableData2;
	}
}
