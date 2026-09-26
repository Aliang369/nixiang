using System;

[Serializable]
public class zbWallSubjectRelicCsvTableData
{
	public zbWallSubjectRelicCsvRecord[] records;

	public static zbWallSubjectRelicCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWallSubjectRelicCsvTableData zbWallSubjectRelicCsvTableData2 = new zbWallSubjectRelicCsvTableData();
		zbWallSubjectRelicCsvRecord[] array = new zbWallSubjectRelicCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbWallSubjectRelicCsvRecord.Read(zbBinReader2);
		}
		zbWallSubjectRelicCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbWallSubjectRelicCsvTableData2;
	}
}
