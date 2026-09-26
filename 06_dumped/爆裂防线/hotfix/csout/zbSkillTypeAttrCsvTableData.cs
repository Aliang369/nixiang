using System;

[Serializable]
public class zbSkillTypeAttrCsvTableData
{
	public zbSkillTypeAttrCsvRecord[] records;

	public static zbSkillTypeAttrCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSkillTypeAttrCsvTableData zbSkillTypeAttrCsvTableData2 = new zbSkillTypeAttrCsvTableData();
		zbSkillTypeAttrCsvRecord[] array = new zbSkillTypeAttrCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbSkillTypeAttrCsvRecord.Read(zbBinReader2);
		}
		zbSkillTypeAttrCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbSkillTypeAttrCsvTableData2;
	}
}
