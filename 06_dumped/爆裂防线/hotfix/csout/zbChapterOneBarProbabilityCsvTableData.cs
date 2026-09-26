using System;

[Serializable]
public class zbChapterOneBarProbabilityCsvTableData
{
	public zbChapterOneBarProbabilityCsvRecord[] records;

	public static zbChapterOneBarProbabilityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbChapterOneBarProbabilityCsvTableData zbChapterOneBarProbabilityCsvTableData2 = new zbChapterOneBarProbabilityCsvTableData();
		zbChapterOneBarProbabilityCsvRecord[] array = new zbChapterOneBarProbabilityCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbChapterOneBarProbabilityCsvRecord.Read(zbBinReader2);
		}
		zbChapterOneBarProbabilityCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbChapterOneBarProbabilityCsvTableData2;
	}
}
