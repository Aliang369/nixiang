using System;

[Serializable]
public class zbChapterThreeBarTargetCsvTableData
{
	public zbChapterThreeBarTargetCsvRecord[] records;

	public static zbChapterThreeBarTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterThreeBarTargetCsvTableData zbChapterThreeBarTargetCsvTableData2 = new zbChapterThreeBarTargetCsvTableData();
		zbChapterThreeBarTargetCsvRecord[] array = new zbChapterThreeBarTargetCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterThreeBarTargetCsvRecord.Read(zbBinReader2);
		}
		zbChapterThreeBarTargetCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterThreeBarTargetCsvTableData2;
	}
}
