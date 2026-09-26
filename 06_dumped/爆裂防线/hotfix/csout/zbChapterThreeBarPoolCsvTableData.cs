using System;

[Serializable]
public class zbChapterThreeBarPoolCsvTableData
{
	public zbChapterThreeBarPoolCsvRecord[] records;

	public static zbChapterThreeBarPoolCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterThreeBarPoolCsvTableData zbChapterThreeBarPoolCsvTableData2 = new zbChapterThreeBarPoolCsvTableData();
		zbChapterThreeBarPoolCsvRecord[] array = new zbChapterThreeBarPoolCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterThreeBarPoolCsvRecord.Read(zbBinReader2);
		}
		zbChapterThreeBarPoolCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterThreeBarPoolCsvTableData2;
	}
}
