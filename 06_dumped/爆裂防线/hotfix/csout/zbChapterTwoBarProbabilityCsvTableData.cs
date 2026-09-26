using System;

[Serializable]
public class zbChapterTwoBarProbabilityCsvTableData
{
	public zbChapterTwoBarProbabilityCsvRecord[] records;

	public static zbChapterTwoBarProbabilityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterTwoBarProbabilityCsvTableData zbChapterTwoBarProbabilityCsvTableData2 = new zbChapterTwoBarProbabilityCsvTableData();
		zbChapterTwoBarProbabilityCsvRecord[] array = new zbChapterTwoBarProbabilityCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterTwoBarProbabilityCsvRecord.Read(zbBinReader2);
		}
		zbChapterTwoBarProbabilityCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterTwoBarProbabilityCsvTableData2;
	}
}
