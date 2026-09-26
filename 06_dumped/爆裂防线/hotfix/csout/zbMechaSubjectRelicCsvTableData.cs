using System;

[Serializable]
public class zbMechaSubjectRelicCsvTableData
{
	public zbMechaSubjectRelicCsvRecord[] records;

	public static zbMechaSubjectRelicCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaSubjectRelicCsvTableData zbMechaSubjectRelicCsvTableData2 = new zbMechaSubjectRelicCsvTableData();
		zbMechaSubjectRelicCsvRecord[] array = new zbMechaSubjectRelicCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbMechaSubjectRelicCsvRecord.Read(zbBinReader2);
		}
		zbMechaSubjectRelicCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbMechaSubjectRelicCsvTableData2;
	}
}
