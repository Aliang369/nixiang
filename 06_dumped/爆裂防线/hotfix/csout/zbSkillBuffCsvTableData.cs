using System;
using System.Collections.Generic;

[Serializable]
public class zbSkillBuffCsvTableData
{
	public Dictionary<int, zbSkillBuffCsvRecord> records = new Dictionary<int, zbSkillBuffCsvRecord>();

	public static zbSkillBuffCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSkillBuffCsvTableData zbSkillBuffCsvTableData2 = new zbSkillBuffCsvTableData();
		Dictionary<int, zbSkillBuffCsvRecord> dictionary = new Dictionary<int, zbSkillBuffCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSkillBuffCsvRecord.Read(zbBinReader2);
		}
		zbSkillBuffCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSkillBuffCsvTableData2;
	}
}
