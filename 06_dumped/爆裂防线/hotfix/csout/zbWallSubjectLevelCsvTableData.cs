using System;

[Serializable]
public class zbWallSubjectLevelCsvTableData
{
	public zbWallSubjectLevelCsvRecord[] records;

	public static zbWallSubjectLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWallSubjectLevelCsvTableData zbWallSubjectLevelCsvTableData2 = new zbWallSubjectLevelCsvTableData();
		zbWallSubjectLevelCsvRecord[] array = new zbWallSubjectLevelCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbWallSubjectLevelCsvRecord.Read(zbBinReader2);
		}
		zbWallSubjectLevelCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbWallSubjectLevelCsvTableData2;
	}
}
