using System;

[Serializable]
public class zbMechaSubjectLevelCsvTableData
{
	public zbMechaSubjectLevelCsvRecord[] records;

	public static zbMechaSubjectLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaSubjectLevelCsvTableData zbMechaSubjectLevelCsvTableData2 = new zbMechaSubjectLevelCsvTableData();
		zbMechaSubjectLevelCsvRecord[] array = new zbMechaSubjectLevelCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbMechaSubjectLevelCsvRecord.Read(zbBinReader2);
		}
		zbMechaSubjectLevelCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbMechaSubjectLevelCsvTableData2;
	}
}
