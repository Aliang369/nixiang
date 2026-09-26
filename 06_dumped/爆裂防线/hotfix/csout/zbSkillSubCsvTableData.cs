using System;
using System.Collections.Generic;

[Serializable]
public class zbSkillSubCsvTableData
{
	public Dictionary<int, zbSkillSubCsvRecord> records = new Dictionary<int, zbSkillSubCsvRecord>();

	public static zbSkillSubCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSkillSubCsvTableData zbSkillSubCsvTableData2 = new zbSkillSubCsvTableData();
		Dictionary<int, zbSkillSubCsvRecord> dictionary = new Dictionary<int, zbSkillSubCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSkillSubCsvRecord.Read(zbBinReader2);
		}
		zbSkillSubCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSkillSubCsvTableData2;
	}
}
