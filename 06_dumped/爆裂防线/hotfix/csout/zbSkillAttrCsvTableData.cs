using System;
using System.Collections.Generic;

[Serializable]
public class zbSkillAttrCsvTableData
{
	public Dictionary<int, zbSkillAttrCsvRecord> records = new Dictionary<int, zbSkillAttrCsvRecord>();

	public static zbSkillAttrCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSkillAttrCsvTableData zbSkillAttrCsvTableData2 = new zbSkillAttrCsvTableData();
		Dictionary<int, zbSkillAttrCsvRecord> dictionary = new Dictionary<int, zbSkillAttrCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSkillAttrCsvRecord.Read(zbBinReader2);
		}
		zbSkillAttrCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSkillAttrCsvTableData2;
	}
}
