using System;

[Serializable]
public class zbChapterThreeBarProbabilityCsvTableData
{
	public zbChapterThreeBarProbabilityCsvRecord[] records;

	public static zbChapterThreeBarProbabilityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterThreeBarProbabilityCsvTableData zbChapterThreeBarProbabilityCsvTableData2 = new zbChapterThreeBarProbabilityCsvTableData();
		zbChapterThreeBarProbabilityCsvRecord[] array = new zbChapterThreeBarProbabilityCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterThreeBarProbabilityCsvRecord.Read(zbBinReader2);
		}
		zbChapterThreeBarProbabilityCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterThreeBarProbabilityCsvTableData2;
	}
}
