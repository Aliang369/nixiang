using System;

[Serializable]
public class zbBarProbabilityCsvTableData
{
	public zbBarProbabilityCsvRecord[] records;

	public static zbBarProbabilityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBarProbabilityCsvTableData zbBarProbabilityCsvTableData2 = new zbBarProbabilityCsvTableData();
		zbBarProbabilityCsvRecord[] array = new zbBarProbabilityCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbBarProbabilityCsvRecord.Read(zbBinReader2);
		}
		zbBarProbabilityCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbBarProbabilityCsvTableData2;
	}
}
