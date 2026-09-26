using System;
using System.Collections.Generic;

[Serializable]
public class zbSkillEffectCsvTableData
{
	public Dictionary<int, zbSkillEffectCsvRecord> records = new Dictionary<int, zbSkillEffectCsvRecord>();

	public static zbSkillEffectCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSkillEffectCsvTableData zbSkillEffectCsvTableData2 = new zbSkillEffectCsvTableData();
		Dictionary<int, zbSkillEffectCsvRecord> dictionary = new Dictionary<int, zbSkillEffectCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSkillEffectCsvRecord.Read(zbBinReader2);
		}
		zbSkillEffectCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSkillEffectCsvTableData2;
	}
}
